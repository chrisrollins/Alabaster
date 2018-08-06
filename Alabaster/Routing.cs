using System;
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
    public delegate void RouteCallback_C();
    public delegate T RouteCallback_D<T>(Request req) where T : struct;
    public delegate IEnumerable<T> RouteCallback_E<T>(Request req) where T : struct;
    public delegate IEnumerable<T> RouteCallback_F<T>() where T : struct;
    
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
        public Controller(HTTPMethod method, string route, RouteCallback_B callback) : this(method, route, Server.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, RouteCallback_C callback) : this(method, route, Server.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, Response res) : this(method, route, Server.ResponseShortcut(res)) { }
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_A c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_B c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_C c) args) => new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, Response res) args) => new Controller(args.m, args.r, args.res);
    }

    public struct URLPatternMatch
    {
        public string PatternSpecifier;
        public URLPatternMatch(string specifier) => this.PatternSpecifier = specifier;
        public static explicit operator URLPatternMatch(string specifier) => new URLPatternMatch(specifier);
    }
    
    public partial class Server
    {
        public static void Routes(params Controller[] controllers) =>                                                   Array.ForEach(controllers, (Controller c) => Routing.AddHandler(c.Method, (RouteArg)c.Route, c.Callback));
        public static void Get(string route, RouteCallback_A callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Get(string route, RouteCallback_B callback) =>                                               Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Get(string route, RouteCallback_C callback) =>                                               Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Get<T>(string route, RouteCallback_D<T> callback) where T : struct =>                        Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Get<T>(string route, RouteCallback_E<T> callback) where T : struct =>                        Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Get<T>(string route, RouteCallback_F<T> callback) where T : struct =>                        Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Get(string route, Response res) =>                                                           Routing.AddHandler((RouteArg)route, ResponseShortcut(res));
        public static void Post(string route, RouteCallback_A callback) =>                                              Routing.AddHandler((RouteArg)route, callback);
        public static void Post(string route, RouteCallback_B callback) =>                                              Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Post(string route, RouteCallback_C callback) =>                                              Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Post<T>(string route, RouteCallback_D<T> callback) where T : struct =>                       Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Post<T>(string route, RouteCallback_E<T> callback) where T : struct =>                       Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Post<T>(string route, RouteCallback_F<T> callback) where T : struct =>                       Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Post(string route, Response res) =>                                                          Routing.AddHandler((RouteArg)route, ResponseShortcut(res));
        public static void Patch(string route, RouteCallback_A callback) =>                                             Routing.AddHandler((RouteArg)route, callback);
        public static void Patch(string route, RouteCallback_B callback) =>                                             Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Patch(string route, RouteCallback_C callback) =>                                             Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Patch<T>(string route, RouteCallback_D<T> callback) where T : struct =>                      Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Patch<T>(string route, RouteCallback_E<T> callback) where T : struct =>                      Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Patch<T>(string route, RouteCallback_F<T> callback) where T : struct =>                      Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Patch(string route, Response res) =>                                                         Routing.AddHandler((RouteArg)route, ResponseShortcut(res));
        public static void Put(string route, RouteCallback_A callback) =>                                               Routing.AddHandler((RouteArg)route, callback);
        public static void Put(string route, RouteCallback_B callback) =>                                               Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Put(string route, RouteCallback_C callback) =>                                               Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Put<T>(string route, RouteCallback_D<T> callback) where T : struct =>                        Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Put<T>(string route, RouteCallback_E<T> callback) where T : struct =>                        Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Put<T>(string route, RouteCallback_F<T> callback) where T : struct =>                        Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Put(string route, Response res) =>                                                           Routing.AddHandler((RouteArg)route, ResponseShortcut(res));
        public static void Delete(string route, RouteCallback_A callback) =>                                            Routing.AddHandler((RouteArg)route, callback);
        public static void Delete(string route, RouteCallback_B callback) =>                                            Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Delete(string route, RouteCallback_C callback) =>                                            Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Delete<T>(string route, RouteCallback_D<T> callback) where T : struct =>                     Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Delete<T>(string route, RouteCallback_E<T> callback) where T : struct =>                     Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Delete<T>(string route, RouteCallback_F<T> callback) where T : struct =>                     Routing.AddHandler((RouteArg)route, Convert(callback));
        public static void Delete(string route, Response res) =>                                                        Routing.AddHandler((RouteArg)route, ResponseShortcut(res));
        public static void Route(string route, RouteCallback_A callback) =>                                             AllMethodRoute((RouteArg)route, callback);
        public static void Route(string route, RouteCallback_B callback) =>                                             AllMethodRoute((RouteArg)route, Convert(callback));
        public static void Route(string route, RouteCallback_C callback) =>                                             AllMethodRoute((RouteArg)route, Convert(callback));
        public static void Route<T>(string route, RouteCallback_D<T> callback) where T : struct =>                      AllMethodRoute((RouteArg)route, Convert(callback));
        public static void Route<T>(string route, RouteCallback_E<T> callback) where T : struct =>                      AllMethodRoute((RouteArg)route, Convert(callback));
        public static void Route(string route, Response res) =>                                                         AllMethodRoute((RouteArg)route, ResponseShortcut(res));
        public static void Route<T>(string route, RouteCallback_F<T> callback) where T : struct =>                      AllMethodRoute((RouteArg)route, Convert(callback));
        public static void Route(string method, string route, RouteCallback_A callback) =>                              Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, RouteCallback_B callback) =>                              Routing.AddHandler((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void Route(string method, string route, RouteCallback_C callback) =>                              Routing.AddHandler((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_D<T> callback) where T : struct =>       Routing.AddHandler((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_E<T> callback) where T : struct =>       Routing.AddHandler((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_F<T> callback) where T : struct =>       Routing.AddHandler((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void Route(string method, string route, Response res) =>                                          Routing.AddHandler((MethodArg)method, (RouteArg)route, ResponseShortcut(res));
        public static void Route(HTTPMethod method, string route, RouteCallback_B callback) =>                          Routing.AddHandler(method, (RouteArg)route, Convert(callback));
        public static void Route(HTTPMethod method, string route, RouteCallback_C callback) =>                          Routing.AddHandler(method, (RouteArg)route, Convert(callback));
        public static void Route<T>(HTTPMethod method, string route, RouteCallback_D<T> callback) where T : struct =>   Routing.AddHandler(method, (RouteArg)route, Convert(callback));
        public static void Route<T>(HTTPMethod method, string route, RouteCallback_E<T> callback) where T : struct =>   Routing.AddHandler(method, (RouteArg)route, Convert(callback));
        public static void Route<T>(HTTPMethod method, string route, RouteCallback_F<T> callback) where T : struct =>   Routing.AddHandler(method, (RouteArg)route, Convert(callback));
        public static void Route(HTTPMethod method, string route, Response res) =>                                      Routing.AddHandler(method, (RouteArg)route, ResponseShortcut(res));
        public static void Route(HTTPMethod method, string route, RouteCallback_A callback) =>                          Routing.AddHandler(method, (RouteArg)route, callback);
        public static void All(HTTPMethod method, RouteCallback_B callback) =>                                          Routing.AddHandler(method, Convert(callback));
        public static void All(HTTPMethod method, RouteCallback_C callback) =>                                          Routing.AddHandler(method, Convert(callback));
        public static void All<T>(HTTPMethod method, RouteCallback_D<T> callback) where T : struct =>                   Routing.AddHandler(method, Convert(callback));
        public static void All<T>(HTTPMethod method, RouteCallback_E<T> callback) where T : struct =>                   Routing.AddHandler(method, Convert(callback));
        public static void All<T>(HTTPMethod method, RouteCallback_F<T> callback) where T : struct =>                   Routing.AddHandler(method, Convert(callback));
        public static void All(HTTPMethod method, Response res) =>                                                      Routing.AddHandler(method, ResponseShortcut(res));
        public static void All(HTTPMethod method, RouteCallback_A callback) =>                                          Routing.AddHandler(method, callback);
        public static void All(RouteCallback_A callback) =>                                                             Routing.AddHandler(callback);
        public static void All(RouteCallback_B callback) =>                                                             Routing.AddHandler(Convert(callback));
        public static void All(RouteCallback_C callback) =>                                                             Routing.AddHandler(Convert(callback));
        public static void All<T>(RouteCallback_D<T> callback) where T : struct =>                                      Routing.AddHandler(Convert(callback));
        public static void All<T>(RouteCallback_E<T> callback) where T : struct =>                                      Routing.AddHandler(Convert(callback));
        public static void All<T>(RouteCallback_F<T> callback) where T : struct =>                                      Routing.AddHandler(Convert(callback));
        public static void All(Response res) =>                                                                         Routing.AddHandler(ResponseShortcut(res));

        private static void AllMethodRoute(RouteArg route, RouteCallback_A callback)                                    { foreach (HTTPMethod method in Enum.GetValues(typeof(HTTPMethod))) { Routing.AddHandler(method, route, callback); } }

        internal static RouteCallback_A Convert(RouteCallback_B callback) =>                                            (Request req) => { callback(req); return new PassThrough(); };
        internal static RouteCallback_A Convert(RouteCallback_C callback) =>                                            (Request req) => { callback(); return new PassThrough(); };
        internal static RouteCallback_A Convert<T>(RouteCallback_D<T> callback) where T : struct =>                     (Request req) => new StringResponse(callback(req).ToString());
        internal static RouteCallback_A Convert<T>(RouteCallback_E<T> callback) where T : struct =>                     (Request req) => new StringResponse(String.Join(null, callback(req) ?? new T[] { }));
        internal static RouteCallback_A Convert<T>(RouteCallback_F<T> callback) where T : struct =>                     (Request req) => new StringResponse(String.Join(null, callback() ?? new T[] { }));
        internal static RouteCallback_A ResponseShortcut(Response res) =>                                               (Request req) => res;

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

            internal static void AddHandler(MethodArg method, RouteCallback_A callback) => AddHandler(method, (RouteArg)null, callback);
            internal static void AddHandler(RouteArg route, RouteCallback_A callback) => AddHandler((MethodArg)null, route, callback);
            internal static void AddHandler(RouteCallback_A callback) => AddHandler((MethodArg)null, (RouteArg)null, callback);
            internal static void AddHandler(MethodArg method, RouteArg route, RouteCallback_A callback)
            {
                Util.InitExceptions();
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
                    Dictionary<string, RouteCallback_A> dict = new Dictionary<string, RouteCallback_A>(Enum.GetValues(typeof(HTTPMethod)).Length);
                    do
                    {
                        (MethodArg m, RouteArg r, RouteCallback_A c) = currentHandlerGroup.Dequeue();
                        string key = m.Value;
                        dict[key] = (dict.TryGetValue(key, out RouteCallback_A existingCallback)) ? (Request req) => existingCallback(req) ?? c(req) : c;
                    } while (currentHandlerGroup.Count > 0);
                    Handlers.Add((Request req) => dict.TryGetValue(req.HttpMethod, out RouteCallback_A handler) ? handler(req) : new PassThrough());
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
