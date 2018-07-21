﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public delegate Response RouteCallback_A(Request req);
    public delegate void RouteCallback_B(Request req);
    public delegate void RouteCallback_C();
    public delegate T RouteCallback_D<T>(Request req) where T : struct;
    public delegate IEnumerable<T> RouteCallback_E<T>(Request req) where T : struct;
    public delegate IEnumerable<T> RouteCallback_F<T>() where T : struct;
    
    public partial class Server
    {      
        public static void Get(string route, RouteCallback_A callback) =>                                           Routing.GetBase((RouteArg)route, callback);
        public static void Post(string route, RouteCallback_A callback) =>                                          Routing.PostBase((RouteArg)route, callback);
        public static void Patch(string route, RouteCallback_A callback) =>                                         Routing.PatchBase((RouteArg)route, callback);
        public static void Put(string route, RouteCallback_A callback) =>                                           Routing.PutBase((RouteArg)route, callback);
        public static void Delete(string route, RouteCallback_A callback) =>                                        Routing.DeleteBase((RouteArg)route, callback);
        public static void Route(string method, string route, RouteCallback_A callback) =>                          Routing.RouteBase((MethodArg)method, (RouteArg)route, callback);
        public static void All(string method, RouteCallback_A callback) =>                                          Routing.AllMethodBase((MethodArg)method, callback);
        public static void All(RouteCallback_A callback) =>                                                         Routing.AllBase(callback);

        public static void Get(string route, RouteCallback_B callback) =>                                           Routing.GetBase((RouteArg)route, Convert(callback));
        public static void Post(string route, RouteCallback_B callback) =>                                          Routing.PostBase((RouteArg)route, Convert(callback));
        public static void Patch(string route, RouteCallback_B callback) =>                                         Routing.PatchBase((RouteArg)route, Convert(callback));
        public static void Put(string route, RouteCallback_B callback) =>                                           Routing.PutBase((RouteArg)route, Convert(callback));
        public static void Delete(string route, RouteCallback_B callback) =>                                        Routing.DeleteBase((RouteArg)route, Convert(callback));
        public static void Route(string method, string route, RouteCallback_B callback) =>                          Routing.RouteBase((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void All(string method, RouteCallback_B callback) =>                                          Routing.AllMethodBase((MethodArg)method, Convert(callback));
        public static void All(RouteCallback_B callback) =>                                                         Routing.AllBase(Convert(callback));

        public static void Get(string route, RouteCallback_C callback) =>                                           Routing.GetBase((RouteArg)route, Convert(callback));
        public static void Post(string route, RouteCallback_C callback) =>                                          Routing.PostBase((RouteArg)route, Convert(callback));
        public static void Patch(string route, RouteCallback_C callback) =>                                         Routing.PatchBase((RouteArg)route, Convert(callback));
        public static void Put(string route, RouteCallback_C callback) =>                                           Routing.PutBase((RouteArg)route, Convert(callback));
        public static void Delete(string route, RouteCallback_C callback) =>                                        Routing.DeleteBase((RouteArg)route, Convert(callback));
        public static void Route(string method, string route, RouteCallback_C callback) =>                          Routing.RouteBase((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void All(string method, RouteCallback_C callback) =>                                          Routing.AllMethodBase((MethodArg)method, Convert(callback));
        public static void All(RouteCallback_C callback) =>                                                         Routing.AllBase(Convert(callback));

        public static void Get<T>(string route, RouteCallback_D<T> callback) where T : struct =>                    Routing.GetBase((RouteArg)route, Convert(callback));
        public static void Post<T>(string route, RouteCallback_D<T> callback) where T : struct =>                   Routing.PostBase((RouteArg)route, Convert(callback));
        public static void Patch<T>(string route, RouteCallback_D<T> callback) where T : struct =>                  Routing.PatchBase((RouteArg)route, Convert(callback));
        public static void Put<T>(string route, RouteCallback_D<T> callback) where T : struct =>                    Routing.PutBase((RouteArg)route, Convert(callback));
        public static void Delete<T>(string route, RouteCallback_D<T> callback) where T : struct =>                 Routing.DeleteBase((RouteArg)route, Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_D<T> callback) where T : struct =>   Routing.RouteBase((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void All<T>(string method, RouteCallback_D<T> callback) where T : struct =>                   Routing.AllMethodBase((MethodArg)method, Convert(callback));
        public static void All<T>(RouteCallback_D<T> callback) where T : struct =>                                  Routing.AllBase(Convert(callback));

        public static void Get<T>(string route, RouteCallback_E<T> callback) where T : struct =>                    Routing.GetBase((RouteArg)route, Convert(callback));
        public static void Post<T>(string route, RouteCallback_E<T> callback) where T : struct =>                   Routing.PostBase((RouteArg)route, Convert(callback));
        public static void Patch<T>(string route, RouteCallback_E<T> callback) where T : struct =>                  Routing.PatchBase((RouteArg)route, Convert(callback));
        public static void Put<T>(string route, RouteCallback_E<T> callback) where T : struct =>                    Routing.PutBase((RouteArg)route, Convert(callback));
        public static void Delete<T>(string route, RouteCallback_E<T> callback) where T : struct =>                 Routing.DeleteBase((RouteArg)route, Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_E<T> callback) where T : struct =>   Routing.RouteBase((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void All<T>(string method, RouteCallback_E<T> callback) where T : struct =>                   Routing.AllMethodBase((MethodArg)method, Convert(callback));
        public static void All<T>(RouteCallback_E<T> callback) where T : struct =>                                  Routing.AllBase(Convert(callback));

        public static void Get<T>(string route, RouteCallback_F<T> callback) where T : struct =>                    Routing.GetBase((RouteArg)route, Convert(callback));
        public static void Post<T>(string route, RouteCallback_F<T> callback) where T : struct =>                   Routing.PostBase((RouteArg)route, Convert(callback));
        public static void Patch<T>(string route, RouteCallback_F<T> callback) where T : struct =>                  Routing.PatchBase((RouteArg)route, Convert(callback));
        public static void Put<T>(string route, RouteCallback_F<T> callback) where T : struct =>                    Routing.PutBase((RouteArg)route, Convert(callback));
        public static void Delete<T>(string route, RouteCallback_F<T> callback) where T : struct =>                 Routing.DeleteBase((RouteArg)route, Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_F<T> callback) where T : struct =>   Routing.RouteBase((MethodArg)method, (RouteArg)route, Convert(callback));
        public static void All<T>(string method, RouteCallback_F<T> callback) where T : struct =>                   Routing.AllMethodBase((MethodArg)method, Convert(callback));
        public static void All<T>(RouteCallback_F<T> callback) where T : struct =>                                  Routing.AllBase(Convert(callback));

        public static void Get(string route, Response res) =>                                                       Routing.GetBase((RouteArg)route, ResponseShortcut(res));
        public static void Post(string route, Response res) =>                                                      Routing.PostBase((RouteArg)route, ResponseShortcut(res));
        public static void Patch(string route, Response res) =>                                                     Routing.PatchBase((RouteArg)route, ResponseShortcut(res));
        public static void Put(string route, Response res) =>                                                       Routing.PutBase((RouteArg)route, ResponseShortcut(res));
        public static void Delete(string route, Response res) =>                                                    Routing.DeleteBase((RouteArg)route, ResponseShortcut(res));
        public static void Route(string method, string route, Response res) =>                                      Routing.RouteBase((MethodArg)method, (RouteArg)route, ResponseShortcut(res));
        public static void All(string method, Response res) =>                                                      Routing.AllMethodBase((MethodArg)method, ResponseShortcut(res));
        public static void All(Response res) =>                                                                     Routing.AllBase(ResponseShortcut(res));
        
        private static RouteCallback_A Convert(RouteCallback_B callback) =>                                         (Request req) => { callback(req); return new PassThrough(); };
        private static RouteCallback_A Convert(RouteCallback_C callback) =>                                         (Request req) => { callback(); return new PassThrough(); };
        private static RouteCallback_A Convert<T>(RouteCallback_D<T> callback) where T : struct =>                  (Request req) => new StringResponse(callback(req).ToString());
        private static RouteCallback_A Convert<T>(RouteCallback_E<T> callback) where T : struct =>                  (Request req) => new StringResponse(String.Join(null, callback(req) ?? new T[] { }));
        private static RouteCallback_A Convert<T>(RouteCallback_F<T> callback) where T : struct =>                  (Request req) => new StringResponse(String.Join(null, callback() ?? new T[] { }));
        private static RouteCallback_A ResponseShortcut(Response res) =>                                            (Request req) => res;
        
        private struct MethodArg
        {
            public static readonly MethodArg Get =       (MethodArg)"GET";
            public static readonly MethodArg Post =      (MethodArg)"POST";
            public static readonly MethodArg Patch =     (MethodArg)"PATCH";
            public static readonly MethodArg Put =       (MethodArg)"PUT";
            public static readonly MethodArg Delete =    (MethodArg)"DELETE";
            public static readonly MethodArg Head =      (MethodArg)"HEAD";
            public static readonly MethodArg Connect =   (MethodArg)"CONNECT";
            public static readonly MethodArg Options =   (MethodArg)"OPTIONS";
            public static readonly MethodArg Trace =     (MethodArg)"TRACE";

            private static readonly string[] standardHTTPMethods = { "GET", "POST", "PATCH", "PUT", "DELETE", "HEAD", "CONNECT", "OPTIONS", "TRACE" };

            public MethodArg(string val)
            {
                if (!Server.Config.EnableCustomHTTPMethods && !standardHTTPMethods.Contains(val.ToUpper())) { throw new ArgumentException("Non-standard HTTP method: " + val + " Enable non-standard HTTP methods to use a custom method by setting Server.EnableCustomHTTPMethods to true."); }
                this.Value = val.ToUpper();
            }
            public readonly string Value;

            public static explicit operator MethodArg(string s) => new MethodArg(s);
        }

        private struct RouteArg
        {
            public RouteArg(string val) => this.Value = string.Join(null, val, (val.Last() != '/') ? "/" : "").ToUpper();            
            public readonly string Value;
            public static explicit operator RouteArg(string s) => new RouteArg(s);
        }

        private static class Routing
        {
            private static Dictionary<RoutingKey, RouteCallback_A> routeCallbacks;
            private static Dictionary<RoutingKey, RouteCallback_A> methodCallbacks;
            private static RouteCallback_A[] UniversalCallbacks;
            private static Queue<RouteCallback_A> deferredUniversalCallbacks = new Queue<RouteCallback_A>();
            private static Queue<Action> deferredRouteCallbacks = new Queue<Action>();

            internal static Response ResolveUniversals(ContextWrapper ctx)
            {
                Response result = null;
                for (int i = 0; i < UniversalCallbacks.Length; i++)
                {
                    result = Resolve(UniversalCallbacks[i], ctx);
                    if (result != null) { return null; }
                }
                return result;
            }

            internal static Response ResolveMethod(ContextWrapper cw) => methodCallbacks.TryGetValue(new RoutingKey(cw), out RouteCallback_A rc) ? Resolve(rc, cw) : null;
            internal static Response ResolveRoute(ContextWrapper cw) => routeCallbacks.TryGetValue(new RoutingKey(cw), out RouteCallback_A rc) ? Resolve(rc, cw, true) : null;

            internal static void GetBase(RouteArg route, RouteCallback_A callback) => Add(MethodArg.Get, route, callback);
            internal static void PostBase(RouteArg route, RouteCallback_A callback) => Add(MethodArg.Post, route, callback);
            internal static void PatchBase(RouteArg route, RouteCallback_A callback) => Add(MethodArg.Patch, route, callback);
            internal static void PutBase(RouteArg route, RouteCallback_A callback) => Add(MethodArg.Put, route, callback);
            internal static void DeleteBase(RouteArg route, RouteCallback_A callback) => Add(MethodArg.Delete, route, callback);
            internal static void RouteBase(MethodArg method, RouteArg route, RouteCallback_A callback) => Add(method, route, callback);
            internal static void AllMethodBase(MethodArg method, RouteCallback_A callback) => AddMethodCallback(method, callback);
            internal static void AllBase(RouteCallback_A callback) => AddUniversalCallback(callback);

            private static void Add(MethodArg method, RouteArg route, RouteCallback_A callback)
            {
                RouteAddingExceptions(method.Value, route.Value);
                deferredRouteCallbacks.Enqueue(() => { routeCallbacks.Add(new RoutingKey(method, route), callback); });
            }

            private static void AddMethodCallback(MethodArg method, RouteCallback_A callback)
            {
                RouteAddingExceptions(method.Value, null);
                deferredRouteCallbacks.Enqueue(() => { methodCallbacks.Add(new RoutingKey(method), callback); });
            }

            private static void AddUniversalCallback(RouteCallback_A callback)
            {
                Util.InitExceptions();
                deferredUniversalCallbacks.Enqueue(callback);
            }

            public static void Activate()
            {
                routeCallbacks = new Dictionary<RoutingKey, RouteCallback_A>(deferredRouteCallbacks.Count);
                UniversalCallbacks = deferredUniversalCallbacks.ToArray();
                while (deferredRouteCallbacks.Count > 0) { deferredRouteCallbacks.Dequeue()(); }
                deferredRouteCallbacks = null;
                deferredUniversalCallbacks = null;
            }

            private static Response Resolve(RouteCallback_A callback, ContextWrapper cw, bool includeRequestWithFileExt = false)
            {
                if (!includeRequestWithFileExt && Util.GetFileExtension(cw.Route) != null) { return null; }
                Response result = callback(new Request(cw));
                result.Merge(cw);
                return (result is PassThrough) ? null : result;
            }

            private static void RouteAddingExceptions(string method, string route)
            {
                Util.InitExceptions();
                if (!isValid(method) || !isValid(route)) { throw new ArgumentException("Routes and methods cannot be empty or contain spaces."); }
                bool isValid(string str) => str == null || str != string.Empty && !str.Contains(' ');
            }
            
            private struct RoutingKey
            {
                private RouteArg route;
                private MethodArg method;

                public RoutingKey(MethodArg method, RouteArg route)
                {
                    this.route = route;
                    this.method = method;
                }
                public RoutingKey(string method, string route) : this((MethodArg)method, (RouteArg)route) { }
                public RoutingKey(MethodArg method) : this(method, new RouteArg("")) { }
                public RoutingKey(RouteArg route) : this(new MethodArg(""), route) { }
                public RoutingKey(ContextWrapper cw) : this(cw.Context.Request.HttpMethod, cw.Context.Request.Url.AbsolutePath) { }
            }
        }
    }
}
