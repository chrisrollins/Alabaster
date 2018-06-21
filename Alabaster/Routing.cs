using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public partial class Server
    {
        public static void AttachWebSocketModule(string route, WebSocketModule module) => Get(route, (Request req) => { if (req.IsWebSocketRequest) { return new WebSocketHandshake(module, req.cw.Context); } else { return new PassThrough(); } });
        public static void Get(string route, Response res) => Get(route, (Request req) => res);
        public static void Get(string route, RouteCallback_A callback) => Routing.Add("GET", route, callback);
        public static void Post(string route, RouteCallback_A callback) => Routing.Add("POST", route, callback);
        public static void Patch(string route, RouteCallback_A callback) => Routing.Add("PATCH", route, callback);
        public static void Put(string route, RouteCallback_A callback) => Routing.Add("PUT", route, callback);
        public static void Delete(string route, RouteCallback_A callback) => Routing.Add("DELETE", route, callback);
        public static void Route(string method, string route, RouteCallback_A callback) => Routing.Add(method, route, callback);
        public static void All(RouteCallback_A callback) => Routing.AddUniversalCallback(callback);
        public static void All(string method, RouteCallback_A callback) => Routing.AddMethodCallback(method, callback);

        public static void Get<T>(string route, RouteCallback_B<T> callback) where T : struct => Get(route, (Request req) => new StringResponse(String.Join(null, callback(req))));
        public static void Post<T>(string route, RouteCallback_B<T> callback) where T : struct => Post(route, (Request req) => new StringResponse(String.Join(null, callback(req))));
        public static void Patch<T>(string route, RouteCallback_B<T> callback) where T : struct => Patch(route, (Request req) => new StringResponse(String.Join(null, callback(req))));
        public static void Put<T>(string route, RouteCallback_B<T> callback) where T : struct => Put(route, (Request req) => new StringResponse(String.Join(null, callback(req))));
        public static void Delete<T>(string route, RouteCallback_B<T> callback) where T : struct => Delete(route, (Request req) => new StringResponse(String.Join(null, callback(req))));
        public static void Route<T>(string route, string method, RouteCallback_B<T> callback) where T : struct => Route(route, method, (Request req) => new StringResponse(String.Join(null, callback(req))));
        public static void All<T>(RouteCallback_B<T> callback) where T : struct => Routing.AddUniversalCallback((Request req) => new StringResponse(String.Join(null, callback(req))));
        public static void All<T>(string method, RouteCallback_A callback) => Routing.AddMethodCallback(method, (Request req) => new StringResponse(String.Join(null, callback(req))));

        public static void Get<T>(string route, RouteCallback_C<T> callback) where T : struct => Get(route, (Request req) => new StringResponse(String.Join(null, callback())));
        public static void Post<T>(string route, RouteCallback_C<T> callback) where T : struct => Post(route, (Request req) => new StringResponse(String.Join(null, callback())));
        public static void Patch<T>(string route, RouteCallback_C<T> callback) where T : struct => Patch(route, (Request req) => new StringResponse(String.Join(null, callback())));
        public static void Put<T>(string route, RouteCallback_C<T> callback) where T : struct => Put(route, (Request req) => new StringResponse(String.Join(null, callback())));
        public static void Delete<T>(string route, RouteCallback_C<T> callback) where T : struct => Delete(route, (Request req) => new StringResponse(String.Join(null, callback())));
        public static void Route<T>(string route, string method, RouteCallback_C<T> callback) where T : struct => Route(route, method, (Request req) => new StringResponse(String.Join(null, callback())));
        public static void All<T>(RouteCallback_C<T> callback) where T : struct => Routing.AddUniversalCallback((Request req) => new StringResponse(String.Join(null, callback())));
        public static void All<T>(string method, RouteCallback_C<T> callback) where T : struct => Routing.AddMethodCallback(method, (Request req) => new StringResponse(String.Join(null, callback())));

        public static void Get(string route, RouteCallback_D callback) => Get(route, (Request req) => { callback(req); return new PassThrough(); });
        public static void Post(string route, RouteCallback_D callback) => Post(route, (Request req) => { callback(req); return new PassThrough(); });
        public static void Patch(string route, RouteCallback_D callback) => Patch(route, (Request req) => { callback(req); return new PassThrough(); });
        public static void Put(string route, RouteCallback_D callback) => Put(route, (Request req) => { callback(req); return new PassThrough(); });
        public static void Delete(string route, RouteCallback_D callback) => Delete(route, (Request req) => { callback(req); return new PassThrough(); });
        public static void Route(string route, string method, RouteCallback_D callback) => Route(route, method, (Request req) => { callback(req); return new PassThrough(); });
        public static void All(RouteCallback_D callback) => Routing.AddUniversalCallback((Request req) => { callback(req); return new PassThrough(); });
        public static void All(string method, RouteCallback_D callback) => Routing.AddMethodCallback(method, (Request req) => { callback(req); return new PassThrough(); });

        private static class Routing
        {
            private static Dictionary<string, RouteCallback_A> routeCallbacks;
            private static Dictionary<string, RouteCallback_A> methodCallbacks;
            private static RouteCallback_A[] UniversalCallbacks;
            private static Queue<RouteCallback_A> deferredUniversalCallbacks = new Queue<RouteCallback_A>();
            private static Queue<Action> deferredMethodCallbacks = new Queue<Action>();
            private static Queue<Action> deferredRouteCallbacks = new Queue<Action>();

            public static Response ResolveUniversals(ContextWrapper ctx)
            {
                Response result = null;
                for (int i = 0; i < UniversalCallbacks.Length; i++)
                {
                    result = Resolve(UniversalCallbacks[i], ctx);
                    if (result == null) { return null; }
                }
                return result;
            }

            public static Response ResolveMethod(ContextWrapper cw) => methodCallbacks.TryGetValue(RouteKey(cw.Context.Request.HttpMethod, ""), out RouteCallback_A rc) ? Resolve(rc, cw) : null;
            public static Response ResolveRoute(ContextWrapper cw) => routeCallbacks.TryGetValue(RouteKey(cw.Context.Request.HttpMethod, cw.Context.Request.Url.AbsolutePath), out RouteCallback_A rc) ? Resolve(rc, cw) : null;

            public static void Add(string method, string route, RouteCallback_A callback)
            {
                RouteAddingExceptions(method ?? "", route ?? "");
                deferredRouteCallbacks.Enqueue(() => { routeCallbacks.Add(RouteKey(method, route), callback); });
            }

            public static void AddMethodCallback(string method, RouteCallback_A callback)
            {
                RouteAddingExceptions(method ?? "", null);
                deferredMethodCallbacks.Enqueue(() => { methodCallbacks.Add(RouteKey(method, ""), callback); });
            }

            public static void AddUniversalCallback(RouteCallback_A callback)
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
