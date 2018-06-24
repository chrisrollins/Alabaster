using System;
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
        public static void Get(string route, RouteCallback_A callback) =>                                           Routing.GetBase(new RouteArg(route), callback);
        public static void Post(string route, RouteCallback_A callback) =>                                          Routing.PostBase(new RouteArg(route), callback);
        public static void Patch(string route, RouteCallback_A callback) =>                                         Routing.PatchBase(new RouteArg(route), callback);
        public static void Put(string route, RouteCallback_A callback) =>                                           Routing.PutBase(new RouteArg(route), callback);
        public static void Delete(string route, RouteCallback_A callback) =>                                        Routing.DeleteBase(new RouteArg(route), callback);
        public static void Route(string method, string route, RouteCallback_A callback) =>                          Routing.RouteBase(new MethodArg(method), new RouteArg(route), callback);
        public static void All(string method, RouteCallback_A callback) =>                                          Routing.AllMethodBase(new MethodArg(method), callback);
        public static void All(RouteCallback_A callback) =>                                                         Routing.AllBase(callback);

        public static void Get(string route, RouteCallback_B callback) =>                                           Routing.GetBase(new RouteArg(route), Convert(callback));
        public static void Post(string route, RouteCallback_B callback) =>                                          Routing.PostBase(new RouteArg(route), Convert(callback));
        public static void Patch(string route, RouteCallback_B callback) =>                                         Routing.PatchBase(new RouteArg(route), Convert(callback));
        public static void Put(string route, RouteCallback_B callback) =>                                           Routing.PutBase(new RouteArg(route), Convert(callback));
        public static void Delete(string route, RouteCallback_B callback) =>                                        Routing.DeleteBase(new RouteArg(route), Convert(callback));
        public static void Route(string method, string route, RouteCallback_B callback) =>                          Routing.RouteBase(new MethodArg(method), new RouteArg(route), Convert(callback));
        public static void All(string method, RouteCallback_B callback) =>                                          Routing.AllMethodBase(new MethodArg(method), Convert(callback));
        public static void All(RouteCallback_B callback) =>                                                         Routing.AllBase(Convert(callback));

        public static void Get(string route, RouteCallback_C callback) =>                                           Routing.GetBase(new RouteArg(route), Convert(callback));
        public static void Post(string route, RouteCallback_C callback) =>                                          Routing.PostBase(new RouteArg(route), Convert(callback));
        public static void Patch(string route, RouteCallback_C callback) =>                                         Routing.PatchBase(new RouteArg(route), Convert(callback));
        public static void Put(string route, RouteCallback_C callback) =>                                           Routing.PutBase(new RouteArg(route), Convert(callback));
        public static void Delete(string route, RouteCallback_C callback) =>                                        Routing.DeleteBase(new RouteArg(route), Convert(callback));
        public static void Route(string method, string route, RouteCallback_C callback) =>                          Routing.RouteBase(new MethodArg(method), new RouteArg(route), Convert(callback));
        public static void All(string method, RouteCallback_C callback) =>                                          Routing.AllMethodBase(new MethodArg(method), Convert(callback));
        public static void All(RouteCallback_C callback) =>                                                         Routing.AllBase(Convert(callback));

        public static void Get<T>(string route, RouteCallback_D<T> callback) where T : struct =>                    Routing.GetBase(new RouteArg(route), Convert(callback));
        public static void Post<T>(string route, RouteCallback_D<T> callback) where T : struct =>                   Routing.PostBase(new RouteArg(route), Convert(callback));
        public static void Patch<T>(string route, RouteCallback_D<T> callback) where T : struct =>                  Routing.PatchBase(new RouteArg(route), Convert(callback));
        public static void Put<T>(string route, RouteCallback_D<T> callback) where T : struct =>                    Routing.PutBase(new RouteArg(route), Convert(callback));
        public static void Delete<T>(string route, RouteCallback_D<T> callback) where T : struct =>                 Routing.DeleteBase(new RouteArg(route), Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_D<T> callback) where T : struct =>   Routing.RouteBase(new MethodArg(method), new RouteArg(route), Convert(callback));
        public static void All<T>(string method, RouteCallback_D<T> callback) where T : struct =>                   Routing.AllMethodBase(new MethodArg(method), Convert(callback));
        public static void All<T>(RouteCallback_D<T> callback) where T : struct =>                                  Routing.AllBase(Convert(callback));

        public static void Get<T>(string route, RouteCallback_E<T> callback) where T : struct =>                    Routing.GetBase(new RouteArg(route), Convert(callback));
        public static void Post<T>(string route, RouteCallback_E<T> callback) where T : struct =>                   Routing.PostBase(new RouteArg(route), Convert(callback));
        public static void Patch<T>(string route, RouteCallback_E<T> callback) where T : struct =>                  Routing.PatchBase(new RouteArg(route), Convert(callback));
        public static void Put<T>(string route, RouteCallback_E<T> callback) where T : struct =>                    Routing.PutBase(new RouteArg(route), Convert(callback));
        public static void Delete<T>(string route, RouteCallback_E<T> callback) where T : struct =>                 Routing.DeleteBase(new RouteArg(route), Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_E<T> callback) where T : struct =>   Routing.RouteBase(new MethodArg(method), new RouteArg(route), Convert(callback));
        public static void All<T>(string method, RouteCallback_E<T> callback) where T : struct =>                   Routing.AllMethodBase(new MethodArg(method), Convert(callback));
        public static void All<T>(RouteCallback_E<T> callback) where T : struct =>                                  Routing.AllBase(Convert(callback));

        public static void Get<T>(string route, RouteCallback_F<T> callback) where T : struct =>                    Routing.GetBase(new RouteArg(route), Convert(callback));
        public static void Post<T>(string route, RouteCallback_F<T> callback) where T : struct =>                   Routing.PostBase(new RouteArg(route), Convert(callback));
        public static void Patch<T>(string route, RouteCallback_F<T> callback) where T : struct =>                  Routing.PatchBase(new RouteArg(route), Convert(callback));
        public static void Put<T>(string route, RouteCallback_F<T> callback) where T : struct =>                    Routing.PutBase(new RouteArg(route), Convert(callback));
        public static void Delete<T>(string route, RouteCallback_F<T> callback) where T : struct =>                 Routing.DeleteBase(new RouteArg(route), Convert(callback));
        public static void Route<T>(string method, string route, RouteCallback_F<T> callback) where T : struct =>   Routing.RouteBase(new MethodArg(method), new RouteArg(route), Convert(callback));
        public static void All<T>(string method, RouteCallback_F<T> callback) where T : struct =>                   Routing.AllMethodBase(new MethodArg(method), Convert(callback));
        public static void All<T>(RouteCallback_F<T> callback) where T : struct =>                                  Routing.AllBase(Convert(callback));

        public static void Get(string route, Response res) =>                                                       Routing.GetBase(new RouteArg(route), ResponseShortcut(res));
        public static void Post(string route, Response res) =>                                                      Routing.PostBase(new RouteArg(route), ResponseShortcut(res));
        public static void Patch(string route, Response res) =>                                                     Routing.PatchBase(new RouteArg(route), ResponseShortcut(res));
        public static void Put(string route, Response res) =>                                                       Routing.PutBase(new RouteArg(route), ResponseShortcut(res));
        public static void Delete(string route, Response res) =>                                                    Routing.DeleteBase(new RouteArg(route), ResponseShortcut(res));
        public static void Route(string method, string route, Response res) =>                                      Routing.RouteBase(new MethodArg(method), new RouteArg(route), ResponseShortcut(res));
        public static void All(string method, Response res) =>                                                      Routing.AllMethodBase(new MethodArg(method), ResponseShortcut(res));
        public static void All(Response res) =>                                                                     Routing.AllBase(ResponseShortcut(res));
        
        private static RouteCallback_A Convert(RouteCallback_B callback) =>                                         (Request req) => { callback(req); return new PassThrough(); };
        private static RouteCallback_A Convert(RouteCallback_C callback) =>                                         (Request req) => { callback(); return new PassThrough(); };
        private static RouteCallback_A Convert<T>(RouteCallback_D<T> callback) where T : struct =>                  (Request req) => new StringResponse(callback(req).ToString());
        private static RouteCallback_A Convert<T>(RouteCallback_E<T> callback) where T : struct =>                  (Request req) => new StringResponse(String.Join(null, callback(req) ?? new T[] { }));
        private static RouteCallback_A Convert<T>(RouteCallback_F<T> callback) where T : struct =>                  (Request req) => new StringResponse(String.Join(null, callback() ?? new T[] { }));
        private static RouteCallback_A ResponseShortcut(Response res) =>                                            (Request req) => res;
        
        private struct MethodArg
        {
            public MethodArg(string val)
            {                
                Util.httpMethodExceptions(val);
                this.Value = val;
            }
            public string Value;
        }

        private struct RouteArg
        {
            public RouteArg(string val) => this.Value = val;
            public string Value;
        }

        private static class Routing
        {
            private static Dictionary<string, RouteCallback_A> routeCallbacks;
            private static Dictionary<string, RouteCallback_A> methodCallbacks;
            private static RouteCallback_A[] UniversalCallbacks;
            private static Queue<RouteCallback_A> deferredUniversalCallbacks = new Queue<RouteCallback_A>();
            private static Queue<Action> deferredMethodCallbacks = new Queue<Action>();
            private static Queue<Action> deferredRouteCallbacks = new Queue<Action>();

            internal static Response ResolveUniversals(ContextWrapper ctx)
            {
                Response result = null;
                for (int i = 0; i < UniversalCallbacks.Length; i++)
                {
                    result = Resolve(UniversalCallbacks[i], ctx);
                    if (result == null) { return null; }
                }
                return result;
            }

            internal static Response ResolveMethod(ContextWrapper cw) => methodCallbacks.TryGetValue(RouteKey(cw.Context.Request.HttpMethod, ""), out RouteCallback_A rc) ? Resolve(rc, cw) : null;
            internal static Response ResolveRoute(ContextWrapper cw) => routeCallbacks.TryGetValue(RouteKey(cw.Context.Request.HttpMethod, cw.Context.Request.Url.AbsolutePath), out RouteCallback_A rc) ? Resolve(rc, cw) : null;

            internal static void GetBase(RouteArg route, RouteCallback_A callback) => Add(new MethodArg("GET"), route, callback);
            internal static void PostBase(RouteArg route, RouteCallback_A callback) => Add(new MethodArg("POST"), route, callback);
            internal static void PatchBase(RouteArg route, RouteCallback_A callback) => Add(new MethodArg("PATCH"), route, callback);
            internal static void PutBase(RouteArg route, RouteCallback_A callback) => Add(new MethodArg("PUT"), route, callback);
            internal static void DeleteBase(RouteArg route, RouteCallback_A callback) => Add(new MethodArg("DELETE"), route, callback);
            internal static void RouteBase(MethodArg method, RouteArg route, RouteCallback_A callback) => Add(method, route, callback);
            internal static void AllMethodBase(MethodArg method, RouteCallback_A callback) => AddMethodCallback(method, callback);
            internal static void AllBase(RouteCallback_A callback) => AddUniversalCallback(callback);

            private static void Add(MethodArg method, RouteArg route, RouteCallback_A callback)
            {
                string m = method.Value ?? "";
                string r = route.Value ?? "";
                RouteAddingExceptions(m, r);
                deferredRouteCallbacks.Enqueue(() => { routeCallbacks.Add(RouteKey(m, r), callback); });
            }

            private static void AddMethodCallback(MethodArg method, RouteCallback_A callback)
            {
                string m = method.Value ?? "";
                RouteAddingExceptions(m, null);
                deferredMethodCallbacks.Enqueue(() => { methodCallbacks.Add(RouteKey(m, ""), callback); });
            }

            private static void AddUniversalCallback(RouteCallback_A callback)
            {
                RouteAddingExceptions(null, null);
                deferredUniversalCallbacks.Enqueue(callback);
            }

            public static void Activate()
            {
                routeCallbacks = new Dictionary<string, RouteCallback_A>(deferredRouteCallbacks.Count);
                methodCallbacks = new Dictionary<string, RouteCallback_A>(deferredMethodCallbacks.Count);

                UniversalCallbacks = deferredUniversalCallbacks.ToArray();
                while (deferredRouteCallbacks.Count > 0) { deferredRouteCallbacks.Dequeue()(); }
                while (deferredMethodCallbacks.Count > 0) { deferredMethodCallbacks.Dequeue()(); }
            }

            private static Response Resolve(RouteCallback_A callback, ContextWrapper cw)
            {
                Response result = callback(new Request(cw));
                result.Merge(cw);
                return (result is PassThrough) ? null : result;
            }

            private static string RouteKey(string method, string route) => String.Join(null, method, route).ToUpper();

            private static void RouteAddingExceptions(string method, string route)
            {
                Util.InitExceptions();
                if (!isValid(method) || !isValid(route)) { throw new ArgumentException("Routes and methods cannot be empty or contain spaces."); }
                bool isValid(string str) => str == null || str != string.Empty && !str.Contains(' ');
            }
        }
    }
}
