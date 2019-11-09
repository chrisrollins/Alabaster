using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Alabaster
{
    [Flags] public enum HTTPScheme : byte { HTTP = 1, HTTPS = 2 };
    public enum HTTPMethod : byte { GET, POST, PATCH, PUT, DELETE, HEAD, CONNECT, OPTIONS, TRACE };

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
                lock (configAccessLock)
                {
                    if (!optionsInitialized) { _config = value; }
                }
            }
        }
        private static HttpListener listener = new HttpListener();
        internal static bool initialized = false;
        internal static volatile bool optionsInitialized = false;
        internal static bool running = false;

        public static void Start(
            int Port,
            string ServerID,
            string StaticFilesBaseDirectory = ServerOptions.Defaults.StaticFilesBaseDirectory,
            HTTPScheme SchemesEnabled = ServerOptions.Defaults.SchemesEnabled,
            long MaximumCacheFileSize = ServerOptions.Defaults.MaximumCacheFileSize,
            bool EnableRouteDiagnostics = ServerOptions.Defaults.EnableRouteDiagnostics,
            bool EnableCustomHTTPMethods = ServerOptions.Defaults.EnableCustomHTTPMethods)
        => Start(new ServerOptions {
            Port = Port,
            ServerID = ServerID,
            StaticFilesBaseDirectory = StaticFilesBaseDirectory,
            SchemesEnabled = SchemesEnabled,
            MaximumCacheFileSize = MaximumCacheFileSize,
            EnableRouteDiagnostics = EnableRouteDiagnostics,
            EnableCustomHTTPMethods = EnableCustomHTTPMethods
        });

        public static void Start(
            int Port,
            string StaticFilesBaseDirectory = ServerOptions.Defaults.StaticFilesBaseDirectory,
            HTTPScheme SchemesEnabled = ServerOptions.Defaults.SchemesEnabled,
            long MaximumCacheFileSize = ServerOptions.Defaults.MaximumCacheFileSize,
            bool EnableRouteDiagnostics = ServerOptions.Defaults.EnableRouteDiagnostics,
            bool EnableCustomHTTPMethods = ServerOptions.Defaults.EnableCustomHTTPMethods)
        => Start(new ServerOptions
        {
            Port = Port,
            ServerID = ServerOptions.Defaults.ServerID,
            StaticFilesBaseDirectory = StaticFilesBaseDirectory,
            SchemesEnabled = SchemesEnabled,
            MaximumCacheFileSize = MaximumCacheFileSize,
            EnableRouteDiagnostics = EnableRouteDiagnostics,
            EnableCustomHTTPMethods = EnableCustomHTTPMethods
        });

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
                if ((Config.SchemesEnabled & HTTPScheme.HTTP) == HTTPScheme.HTTP) { listener.Prefixes.Add(string.Join(null, "http://*:", Config.Port.ToString(), "/")); }
                if ((Config.SchemesEnabled & HTTPScheme.HTTPS) == HTTPScheme.HTTPS) { listener.Prefixes.Add(string.Join(null, "https://*:", Config.Port.ToString(), "/")); }
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
                    FileIO.InitializeFileRequestHandler,
                    Routing.Initialize,
                    FinalizeExceptionHandlers,
                    LaunchListeners,
                    GC.Collect
                );
                initialized = true;
            }

            void InitializeOptions()
            {
                lock (configAccessLock)
                {
                    optionsInitialized = true;
                    Array.ForEach(Config.GetType().GetProperties(), prop => {
                        object temp = prop.GetValue(Config);
                    });
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
                        ContextWrapper cw = new ContextWrapper(listener.GetContext());
                        stp.QueueWork(() => HandleRequest(cw));
                    }
                }

                void HandleRequest(ContextWrapper cw)
                {
                    Response result;
                    try { result = Routing.ResolveHandlers(cw); }
                    catch (Exception e) { result = ResolveException(e, cw); }
                    result.Finish(cw);
                }
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