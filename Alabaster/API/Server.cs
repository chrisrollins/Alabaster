using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Alabaster
{
    public static partial class Server
    {
        private static object configAccessLock = new object();
        private static ServerOptions _config;
        public static ServerOptions Config
        {
            get
            {
                lock (configAccessLock)
                {
                    return _config;
                }
            }
            set
            {
                Util.InitExceptions();
                lock (configAccessLock)
                {
                    _config = value;
                }
            }
        }
        private static HttpListener listener = new HttpListener();
        internal static bool initialized = false;
        internal static bool running = false;
        
        public static void Start(int Port) => Start(new ServerOptions { Port = Port });        

        public static void Start(ServerOptions options)
        {
            Config = options;
            Start();
        }

        public static void Start()
        {
            ServerThreadManager.Run(() =>
            {
                if (!initialized) { Init(); }
                else if (!running) { LaunchListeners(); }
            });

            void Init()
            {
                Util.InitExceptions();
                listener.Prefixes.Add(String.Join(null, "http://*:", Config.Port.ToString(), "/"));
                try { listener.Start(); }
                catch (HttpListenerException e)
                {
                    Console.WriteLine("Server was unable to start. Error code: " + e.ErrorCode);
                    Console.WriteLine("Exception message: " + e.Message);
                    return;
                }
                if (Config.Port == 0) { throw new InvalidOperationException("Port not set."); }
                Util.ProgressVisualizer("Initializing Server...", "Listening on port " + Config.Port,
                    InitializeOptions,
                    FileIO.Initialize,
                    Routing.Initialize,
                    LaunchListeners,
                    GC.Collect
                );
                initialized = true;
            }

            void InitializeOptions()
            {
                foreach (PropertyInfo prop in Config.GetType().GetProperties())
                {
                    object temp = prop.GetValue(Config);
                }
            }

            void LaunchListeners()
            {
                running = true;
                SmartThreadPool stp = new SmartThreadPool(Environment.ProcessorCount, 100, true);
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    new Thread(Listen).Start();
                }

                void Listen()
                {
                    while (running)
                    {
                        HttpListenerContext ctx = Server.listener.GetContext();
                        stp.QueueWork(() => HandleRequest(new ContextWrapper(ctx)));
                    }
                }

                void HandleRequest(ContextWrapper cw)
                {
                    ResponseExceptionHandler(cw, () =>
                        Routing.ResolveHandlers(cw) ??
                        new FileResponse(cw.Route)
                    ).Finish(cw);
                }
            }

            Response ResponseExceptionHandler(ContextWrapper cw, Func<Response> callback)
            {
                Response result;
                try { result = callback(); }
                catch (Exception e)
                {
                    Console.WriteLine("Exception while handling request:");
                    Console.WriteLine(e);
                    Console.WriteLine("Request URL path: \"" + cw.Route + "\"");
                    Console.WriteLine("Request HTTP method: \"" + cw.HttpMethod + "\"");
                    result = new EmptyResponse(500);
                }
                return result;
            }
        }        

        public static void Stop()
        {
            ServerThreadManager.Run(() =>
            {
                if (running) { running = false; }
                else { throw new InvalidOperationException(); }
            });
        }
    }
}