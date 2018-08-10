﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Alabaster
{
    public delegate Response RouteCallback_A(Request req);
    public delegate void RouteCallback_B(Request req);
    public delegate Response RouteCallback_C();
    public delegate void RouteCallback_D();
    public delegate string RouteCallback_E(Request req);
    public delegate string RouteCallback_F();
    
    public enum HTTPMethod : byte { GET, POST, PATCH, PUT, DELETE, HEAD, CONNECT, OPTIONS, TRACE };

    public struct Controller
    {
        public string Route;
        public HTTPMethod Method;
        public RouteCallback_A Callback;
        public Controller(HTTPMethod method, string route, RouteCallback_A callback)
        {
            this.Route = route;
            this.Method = method;
            this.Callback = callback;
        }
        public Controller(HTTPMethod method, string route, RouteCallback_B callback) : this(method, route, Server.RouteCallbackConverter.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, RouteCallback_C callback) : this(method, route, Server.RouteCallbackConverter.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, RouteCallback_D callback) : this(method, route, Server.RouteCallbackConverter.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, RouteCallback_E callback) : this(method, route, Server.RouteCallbackConverter.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, RouteCallback_F callback) : this(method, route, Server.RouteCallbackConverter.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, Response res) : this(method, route, Server.RouteCallbackConverter.ResponseShortcut(res)) { }
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_A c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_B c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_C c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_D c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_E c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_F c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, Response res) args) => new Controller(args.m, args.r, args.res);
    }

    public abstract partial class Response
    {
        public static implicit operator Response(FileIO.FileData file) => new DataResponse(file.Data);
        public static implicit operator Response(byte[] bytes) => new DataResponse(bytes);
        public static implicit operator Response(byte b) => new DataResponse(new byte[] { b });
        public static implicit operator Response(string str) => new StringResponse(str);
        public static implicit operator Response(char c) => new StringResponse(c.ToString());
        public static implicit operator Response(Int64 n) => new StringResponse(n.ToString());
        public static implicit operator Response(Int32 n) => new StringResponse(n.ToString());
        public static implicit operator Response(Int16 n) => new StringResponse(n.ToString());
        public static implicit operator Response(UInt64 n) => new StringResponse(n.ToString());
        public static implicit operator Response(UInt32 n) => new StringResponse(n.ToString());
        public static implicit operator Response(UInt16 n) => new StringResponse(n.ToString());
        public static implicit operator Response(Int64[] arr) => JoinArr(arr);
        public static implicit operator Response(Int32[] arr) => JoinArr(arr);
        public static implicit operator Response(Int16[] arr) => JoinArr(arr);
        public static implicit operator Response(UInt64[] arr) => JoinArr(arr);
        public static implicit operator Response(UInt32[] arr) => JoinArr(arr);
        public static implicit operator Response(UInt16[] arr) => JoinArr(arr);
        public static implicit operator Response(char[] arr) => JoinArr(arr);
        public static implicit operator Response(float[] arr) => JoinArr(arr);
        public static implicit operator Response(double[] arr) => JoinArr(arr);
        public static implicit operator Response(decimal[] arr) => JoinArr(arr);

        private static string JoinArr<T>(T[] arr) => "[" + string.Join(",", arr) + "]";

        void test()
        {
            Server.Get("", () => { return 2; } );
        }
    }
        
    public partial class Server
    {
        public static void Routes(params Controller[] controllers) =>                                                   Array.ForEach(controllers, (Controller c) => Routing.AddHandler(c.Method, (RouteArg)c.Route, c.Callback));
        public static void Get(string route, RouteCallback_A callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Get(string route, RouteCallback_B callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Get(string route, RouteCallback_C callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Get(string route, RouteCallback_D callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Get(string route, Response res) =>                                                           Routing.AddHandler((RouteArg)route, res);
        public static void Post(string route, RouteCallback_A callback) =>                                              Routing.AddHandler((RouteArg)route, callback);
        public static void Post(string route, RouteCallback_B callback) =>                                              Routing.AddHandler((RouteArg)route, callback);
        public static void Post(string route, RouteCallback_C callback) =>                                              Routing.AddHandler((RouteArg)route, callback);
        public static void Post(string route, RouteCallback_D callback) =>                                              Routing.AddHandler((RouteArg)route, callback);
        public static void Post(string route, Response res) =>                                                          Routing.AddHandler((RouteArg)route, res);
        public static void Patch(string route, RouteCallback_A callback) =>                                             Routing.AddHandler((RouteArg)route, callback);
        public static void Patch(string route, RouteCallback_B callback) =>                                             Routing.AddHandler((RouteArg)route, callback);
        public static void Patch(string route, RouteCallback_C callback) =>                                             Routing.AddHandler((RouteArg)route, callback);
        public static void Patch(string route, RouteCallback_D callback) =>                                             Routing.AddHandler((RouteArg)route, callback);
        public static void Patch(string route, Response res) =>                                                         Routing.AddHandler((RouteArg)route, res);
        public static void Put(string route, RouteCallback_A callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Put(string route, RouteCallback_B callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Put(string route, RouteCallback_C callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Put(string route, RouteCallback_D callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Put(string route, Response res) =>                                                           Routing.AddHandler((RouteArg)route, res);
        public static void Delete(string route, RouteCallback_A callback) =>                                            Routing.AddHandler((RouteArg)route, callback);
        public static void Delete(string route, RouteCallback_B callback) =>                                            Routing.AddHandler((RouteArg)route, callback);
        public static void Delete(string route, RouteCallback_C callback) =>                                            Routing.AddHandler((RouteArg)route, callback);
        public static void Delete(string route, RouteCallback_D callback) =>                                            Routing.AddHandler((RouteArg)route, callback);
        public static void Delete(string route, Response res) =>                                                        Routing.AddHandler((RouteArg)route, res);
        public static void Route(string route, RouteCallback_A callback) =>                                             AllMethodRoute((RouteArg)route, callback);
        public static void Route(string route, RouteCallback_B callback) =>                                             AllMethodRoute((RouteArg)route, callback);
        public static void Route(string route, RouteCallback_C callback) =>                                             AllMethodRoute((RouteArg)route, callback);
        public static void Route(string route, RouteCallback_D callback) =>                                             AllMethodRoute((RouteArg)route, callback);
        public static void Route(string route, Response res) =>                                                         AllMethodRoute((RouteArg)route, res);
        public static void Route(string method, string route, RouteCallback_A callback) =>                              Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, RouteCallback_B callback) =>                              Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, RouteCallback_C callback) =>                              Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, RouteCallback_D callback) =>                              Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, Response res) =>                                          Routing.AddHandler((MethodArg)method, (RouteArg)route, res);
        public static void Route(HTTPMethod method, string route, RouteCallback_B callback) =>                          Routing.AddHandler(method, (RouteArg)route, callback);
        public static void Route(HTTPMethod method, string route, RouteCallback_C callback) =>                          Routing.AddHandler(method, (RouteArg)route, callback);
        public static void Route(HTTPMethod method, string route, RouteCallback_D callback) =>                          Routing.AddHandler(method, (RouteArg)route, callback);
        public static void Route(HTTPMethod method, string route, Response res) =>                                      Routing.AddHandler(method, (RouteArg)route, res);
        public static void Route(HTTPMethod method, string route, RouteCallback_A callback) =>                          Routing.AddHandler(method, (RouteArg)route, callback);
        public static void All(HTTPMethod method, RouteCallback_B callback) =>                                          Routing.AddHandler(method, callback);
        public static void All(HTTPMethod method, RouteCallback_C callback) =>                                          Routing.AddHandler(method, callback);
        public static void All(HTTPMethod method, RouteCallback_D callback) =>                                          Routing.AddHandler(method, callback);
        public static void All(HTTPMethod method, Response res) =>                                                      Routing.AddHandler(method, res);
        public static void All(HTTPMethod method, RouteCallback_A callback) =>                                          Routing.AddHandler(method, callback);
        public static void All(RouteCallback_A callback) =>                                                             Routing.AddHandler(callback);
        public static void All(RouteCallback_B callback) =>                                                             Routing.AddHandler(callback);
        public static void All(RouteCallback_C callback) =>                                                             Routing.AddHandler(callback);
        public static void All(RouteCallback_D callback) =>                                                             Routing.AddHandler(callback);
        public static void All(Response res) =>                                                                         Routing.AddHandler(res);

        private static void AllMethodRoute(RouteArg route, RouteCallbackConverter cb)                                   { foreach (HTTPMethod method in Enum.GetValues(typeof(HTTPMethod))) { Routing.AddHandler(method, route, cb); } }
                
        internal struct RouteCallbackConverter
        {
            internal RouteCallback_A Callback;
            internal RouteCallbackConverter(RouteCallback_A cb) => this.Callback = cb;
            public static implicit operator RouteCallbackConverter(RouteCallback_A cb) => new RouteCallbackConverter(cb);
            public static implicit operator RouteCallbackConverter(RouteCallback_B cb) => new RouteCallbackConverter(Convert(cb));
            public static implicit operator RouteCallbackConverter(RouteCallback_C cb) => new RouteCallbackConverter(Convert(cb));
            public static implicit operator RouteCallbackConverter(RouteCallback_D cb) => new RouteCallbackConverter(Convert(cb));
            public static implicit operator RouteCallbackConverter(Response res) => new RouteCallbackConverter(ResponseShortcut(res));

            internal static RouteCallback_A Convert(RouteCallback_B callback) => (Request req) => { callback(req); return new PassThrough(); };
            internal static RouteCallback_A Convert(RouteCallback_C callback) => (Request req) => callback();
            internal static RouteCallback_A Convert(RouteCallback_D callback) => (Request req) => { callback(); return new PassThrough(); };
            internal static RouteCallback_A Convert(RouteCallback_E callback) => (Request req) => new StringResponse(callback(req) ?? "");
            internal static RouteCallback_A Convert(RouteCallback_F callback) => (Request req) => new StringResponse(callback() ?? "");
            internal static RouteCallback_A ResponseShortcut(Response res) => (Request req) => res;
        }

        private struct MethodArg
        {
            public readonly string Value;
            static readonly string[] stdMethods = Enum.GetNames(typeof(HTTPMethod));
            public MethodArg(string val)
            {
                if (!Server.Config.EnableCustomHTTPMethods && !stdMethods.Contains(val.ToUpper())) { throw new ArgumentException("Non-standard HTTP method: \"" + val + "\". Enable non-standard HTTP methods to use a custom method by setting Server.Config.EnableCustomHTTPMethods to true."); }
                this.Value = val.ToUpper();
            }
            public MethodArg(HTTPMethod method) => this.Value = method.ToString();
            public static explicit operator MethodArg(string method) => new MethodArg(method);
            public static implicit operator MethodArg(HTTPMethod method) => new MethodArg(method);
        }

        private struct RouteArg
        {
            public readonly string Value;
            public RouteArg(string val) => this.Value = string.Join(null, val, (val.Last() != '/') ? "/" : "").ToUpper();
            public static explicit operator RouteArg(string s) => new RouteArg(s);
        }  

        private static class Routing
        {
            private static RouteCallback_A[] FinalizedHandlers;
            private static List<RouteCallback_A> Handlers = new List<RouteCallback_A>(1024);
            private static Queue<(MethodArg method, RouteArg route, RouteCallback_A callback)> currentHandlerGroup = new Queue<(MethodArg, RouteArg, RouteCallback_A)>(100);
            private enum HandlerType : byte { Universal, URL, StandardMethod, CustomMethod }
            private static HandlerType? lastHandlerType = null;

            internal static void AddHandler(MethodArg method, RouteCallbackConverter rc) => AddHandler(method, (RouteArg)null, rc);
            internal static void AddHandler(RouteArg route, RouteCallbackConverter rc) => AddHandler((MethodArg)null, route, rc);
            internal static void AddHandler(RouteCallbackConverter rc) => AddHandler((MethodArg)null, (RouteArg)null, rc);
            internal static void AddHandler(MethodArg method, RouteArg route, RouteCallbackConverter rc)
            {
                Util.InitExceptions();
                RouteCallback_A callback = rc.Callback;
                RouteAddingExceptions(method.Value, route.Value, callback);
                HandlerType currentHandlerType = GetHandlerType();

                ServerThreadManager.Run(() =>
                {
                    if (currentHandlerType != lastHandlerType) { ProcessHandlerQueue(); }
                    currentHandlerGroup.Enqueue((method, route, callback));
                    lastHandlerType = currentHandlerType;
                });

                HandlerType GetHandlerType()
                {
                    if (route.Value == null && method.Value == null) { return HandlerType.Universal; }
                    if (route.Value != null) { return HandlerType.URL; }
                    if (Array.IndexOf(Enum.GetNames(typeof(HTTPMethod)), method.Value) != -1) { return HandlerType.StandardMethod; }
                    return HandlerType.CustomMethod;
                }
            }

            private static void ProcessHandlerQueue()
            {
                if (currentHandlerGroup.Count == 0) { return; }

                if (lastHandlerType == HandlerType.Universal)
                {
                    while (currentHandlerGroup.Count > 0)
                    {
                        Handlers.Add(currentHandlerGroup.Dequeue().callback);
                    }
                }
                else if(lastHandlerType == HandlerType.StandardMethod || lastHandlerType == HandlerType.CustomMethod)
                {
                    Dictionary<RoutingKey, RouteCallback_A> dict = new Dictionary<RoutingKey, RouteCallback_A>(Enum.GetValues(typeof(HTTPMethod)).Length);
                    do
                    {
                        (MethodArg m, RouteArg r, RouteCallback_A c) = currentHandlerGroup.Dequeue();
                        RoutingKey key = (MethodArg)m.Value;
                        dict[key] = (dict.TryGetValue(key, out RouteCallback_A existingCallback)) ? (Request req) => existingCallback(req) ?? c(req) : c;
                    } while (currentHandlerGroup.Count > 0);
                    Handlers.Add((Request req) => dict.TryGetValue((MethodArg)req.HttpMethod, out RouteCallback_A handler) ? handler(req) : new PassThrough());
                }
                else
                {
                    Dictionary<RoutingKey, RouteCallback_A> dict = new Dictionary<RoutingKey, RouteCallback_A>(currentHandlerGroup.Count);
                    do
                    {
                        (MethodArg m, RouteArg r, RouteCallback_A c) = currentHandlerGroup.Dequeue();
                        RoutingKey key = (m, r);
                        dict[key] = (dict.TryGetValue(key, out RouteCallback_A existingCallback)) ? (Request req) => existingCallback(req) ?? c(req) : c;                        
                    } while (currentHandlerGroup.Count > 0);
                    Handlers.Add((Request req) => dict.TryGetValue(req.cw, out RouteCallback_A handler) ? handler(req) : new PassThrough());
                }
            }

            internal static void Initialize()
            {
                ProcessHandlerQueue();
                FinalizedHandlers = Handlers.ToArray();
                Handlers = null;
            }

            internal static Response ResolveHandlers(ContextWrapper cw, bool includeRequestWithFileExt = false)
            {
                if (!includeRequestWithFileExt && Util.GetFileExtension(cw.Route) != null) { return null; }

                Response result = null;
                foreach(RouteCallback_A handler in FinalizedHandlers)
                {
                    result = handler(new Request(cw));
                    result.Merge(cw);
                    if(!(result is PassThrough)) { break; }
                }
                return (result is PassThrough) ? null : result;
            }


            private static void RouteAddingExceptions(string method, string route, RouteCallback_A callback)
            {
                Util.InitExceptions();
                if (callback == null) { throw new ArgumentNullException("Callback cannot be null."); }
                if (!isValid(method) || !isValid(route)) { throw new ArgumentException("Routes and methods cannot be empty or contain spaces."); }
                bool isValid(string str) => str == null || str != string.Empty && !str.Contains(' ');
            }

            internal struct RoutingKey
            {
                private (RouteArg route, MethodArg method) data;
                public override int GetHashCode() => data.GetHashCode();
                public RoutingKey(MethodArg method, RouteArg route) => this.data = (route, method);
                public RoutingKey(string method, string route) : this((MethodArg)method, (RouteArg)route) { }
                public RoutingKey(MethodArg method) : this(method, new RouteArg("")) { }
                public RoutingKey(RouteArg route) : this(new MethodArg(""), route) { }
                public RoutingKey(ContextWrapper cw) : this(cw.Context.Request.HttpMethod, cw.Context.Request.Url.AbsolutePath) { }
                public static implicit operator RoutingKey((MethodArg method, RouteArg route) bundle) => new RoutingKey(bundle.method, bundle.route);
                public static implicit operator RoutingKey(MethodArg method) => new RoutingKey(method);
                public static implicit operator RoutingKey(RouteArg route) => new RoutingKey(route);
                public static implicit operator RoutingKey(ContextWrapper cw) => new RoutingKey(cw);
            }
        }
    }
}
