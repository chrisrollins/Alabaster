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
        private static bool running = false;

        private static int port;
        public static int Port
        {
            get => port;
            set
            {
                Util.InitExceptions();
                port = Util.Clamp(value, 0, UInt16.MaxValue);
                Console.WriteLine(port);
            }
        }

        public static bool EnableCustomHTTPMethods = false;

        static Server()
        {
            if(Interlocked.CompareExchange<Thread>(ref baseThread, Thread.CurrentThread, null) != null) { Util.ThreadExceptions(); }
        }

        public static void Start()
        {
            Util.ThreadExceptions();
            if (!initialized) { Init(); }
            else if (!running) { LaunchListeners(); }
            
            void Init()
            {
                Util.InitExceptions();
                initialized = true;
                Util.ProgressVisualizer("Initializing Server...", "Listening on port " + Port,
                    Routing.Activate,
                    LaunchListeners,
                    PreventProgramTermination
                );
            }

            void LaunchListeners()
            {
                running = true;
                listener.Prefixes.Add(String.Join(null, "http://*:", Port.ToString(), "/"));
                listener.Start();
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
                        new FileResponse(cw.Context.Request.Url.AbsolutePath)
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