using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Alabaster
{
    public delegate Response RouteCallback(Request req);

    public static partial class Server
    {
        private static HttpListener listener = new HttpListener();
        private static Thread baseThread = null;
        private static Thread keepAliveThread = null;
        private static bool initialized = false;
        private static bool running = false;

        private static int port;
        public static int Port
        {
            get => port;
            set
            {
                InitExceptions();
                port = Math.Max(0, Math.Min(value, UInt16.MaxValue));
            }
        }

        static Server()
        {
            if(Interlocked.CompareExchange<Thread>(ref baseThread, Thread.CurrentThread, null) != null) { ThreadExceptions(); }
        }

        public static void AttachWebSocketModule(string route, WebSocketModule module) => Get(route, (Request req) => { if (req.IsWebSocketRequest) { return new WebSocketHandshake(module, req.cw.Context); } else { return new PassThrough(); } });
        public static void Get(string route, string file) => Get(route, (Request req) => { return new DataResponse(FileIO.GetStaticFile(file)); });
        public static void Get(string route, RouteCallback callback) => Routing.Add("GET", route, callback);
        public static void Post(string route, RouteCallback callback) => Routing.Add("POST", route, callback);
        public static void Patch(string route, RouteCallback callback) => Routing.Add("PATCH", route, callback);
        public static void Put(string route, RouteCallback callback) => Routing.Add("PUT", route, callback);
        public static void Delete(string route, RouteCallback callback) => Routing.Add("DELETE", route, callback);
        public static void Route(string method, string route, RouteCallback callback) => Routing.Add(method, route, callback);
        public static void All(RouteCallback callback) => Routing.AddUniversalCallback(callback);
        public static void All(string method, RouteCallback callback) => Routing.AddMethodCallback(method, callback);

        public static void Start()
        {
            ThreadExceptions();
            if (!initialized) { Init(); }
            else if (!running) { LaunchListeners(); }
            
            void Init()
            {
                InitExceptions();
                initialized = true;
                ProgressVisualizer("Initializing Server...", "Listening on port " + Port,
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
                    ResponseExceptionHandler( () => Routing.ResolveUniversals(cw)
                    ?? Routing.ResolveMethod(cw)
                    ?? Routing.ResolveRoute(cw)
                    ?? new DataResponse(FileIO.GetStaticFile(cw.Context.Request.Url.AbsolutePath)) ).Finish(cw);
                }
            }

            Response ResponseExceptionHandler(Func<Response> callback)
            {
                Response result;
                try { result = callback(); }
                catch { result = new EmptyResponse(500); }
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
            ThreadExceptions();
            if (running) { running = false; }
            else { throw new InvalidOperationException(); }
        }

        private static void ProgressVisualizer(string startLabel, string endLabel, params Action[] functions)
        {
            Console.WriteLine(startLabel);
            int barLength = 100 + (functions.Length - (100 % functions.Length));
            int chunkSize = barLength / functions.Length;
            string progressChunk = new string(':', chunkSize);
            Console.Write('[');
            Console.CursorLeft = barLength + 1;
            Console.Write(']');
            for (int i = 0; i < functions.Length; i++)
            {
                functions[i]();
                Console.CursorLeft = chunkSize * i + 1;
                Console.Write(progressChunk);
            }
            Console.CursorLeft = 0;
            Console.WriteLine('\n' + endLabel + new string(' ', barLength));
        }

        private static string GetFileExtension(string filename)
        {
            for (int i = filename.Length - 1; i > 0; i--)
            {
                if (filename[i] == '.') { return filename.Substring(i + 1); }
            }
            return null;
        }

        internal static void InitExceptions(Action callback = null)
        {
            ThreadExceptions();
            if (initialized) { throw new InvalidOperationException("Cannot use initialization operations after server has started."); }
            callback?.Invoke();
        }

        internal static void ThreadExceptions(Action callback = null)
        {
            if (baseThread != Thread.CurrentThread) { throw new InvalidOperationException("Server setup must be done on one thread."); }
            callback?.Invoke();
        }
    }
}