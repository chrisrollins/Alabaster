using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Alabaster
{
    public static partial class Server
    {
        private static HttpListener listener = new HttpListener();
        private static Thread keepAliveThread = null;
        internal static Thread baseThread = null;
        internal static bool initialized = false;
        internal static bool running = false;

        static Server()
        {
            if(Interlocked.CompareExchange<Thread>(ref baseThread, Thread.CurrentThread, null) != null) { Util.ThreadExceptions(); }
        }
        
        public static void Start(int Port, bool EnableCustomHTTPMethods = false)
        {
            Config.Port = Port;
            Config.EnableCustomHTTPMethods = EnableCustomHTTPMethods;
            Start();
        }

        public static void Start()
        {
            Util.ThreadExceptions();
            if (!initialized) { Init(); }
            else if (!running) { LaunchListeners(); }
            
            void Init()
            {
                listener.Prefixes.Add(String.Join(null, "http://*:", _port.ToString(), "/"));
                try { listener.Start(); }
                catch (HttpListenerException e)
                {
                    Console.WriteLine("Server was unable to start. Error code: " + e.ErrorCode);
                    Console.WriteLine("Exception message: " + e.Message);
                    return;
                }

                if (Config.Port == 0) { throw new InvalidOperationException("Port not set."); }

                Util.InitExceptions();
                Util.ProgressVisualizer("Initializing Server...", "Listening on port " + _port,
                    Routing.Activate,
                    LaunchListeners,
                    PreventProgramTermination
                );
                initialized = true;
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
                    ResponseExceptionHandler(()=>
                        Routing.ResolveUniversals(cw) ??
                        Routing.ResolveMethod(cw) ??
                        Routing.ResolveRoute(cw) ??
                        new FileResponse(cw.Route)
                    ).Finish(cw);
                }
            }

            Response ResponseExceptionHandler(Func<Response> callback)
            {
                Response result;
                try { result = callback(); }
                catch(Exception e)
                {
                    Console.WriteLine("Exception in application code:");
                    Console.WriteLine(e);
                    result = new EmptyResponse(500);
                }
                return result;
            }

            void PreventProgramTermination()
            {
                keepAliveThread = new Thread(() => Thread.Sleep(Timeout.Infinite));
                keepAliveThread.Start();
            }
        }

        public static void Stop()
        {
            Util.ThreadExceptions();
            if (running) { running = false; }
            else { throw new InvalidOperationException(); }
        }
    }
}