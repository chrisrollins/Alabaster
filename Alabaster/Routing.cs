using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public partial class Server
    {
        private static class Routing
        {
            private static Dictionary<string, RouteCallback> routeCallbacks;
            private static Dictionary<string, RouteCallback> methodCallbacks;
            private static RouteCallback[] UniversalCallbacks;
            private static Queue<RouteCallback> deferredUniversalCallbacks = new Queue<RouteCallback>();
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

            public static Response ResolveMethod(ContextWrapper cw) => methodCallbacks.TryGetValue(RouteKey(cw.Context.Request.HttpMethod, ""), out RouteCallback rc) ? Resolve(rc, cw) : null;
            public static Response ResolveRoute(ContextWrapper cw) => routeCallbacks.TryGetValue(RouteKey(cw.Context.Request.HttpMethod, cw.Context.Request.Url.AbsolutePath), out RouteCallback rc) ? Resolve(rc, cw) : null;

            public static void Add(string method, string route, RouteCallback callback)
            {
                RouteAddingExceptions(method ?? "", route ?? "");
                deferredRouteCallbacks.Enqueue(() => { routeCallbacks.Add(RouteKey(method, route), callback); });
            }

            public static void AddMethodCallback(string method, RouteCallback callback)
            {
                RouteAddingExceptions(method ?? "", null);
                deferredMethodCallbacks.Enqueue(() => { methodCallbacks.Add(RouteKey(method, ""), callback); });
            }

            public static void AddUniversalCallback(RouteCallback callback)
            {
                RouteAddingExceptions(null, null);
                deferredUniversalCallbacks.Enqueue(callback);
            }

            public static void Activate()
            {
                routeCallbacks = new Dictionary<string, RouteCallback>(deferredRouteCallbacks.Count);
                methodCallbacks = new Dictionary<string, RouteCallback>(deferredMethodCallbacks.Count);

                UniversalCallbacks = deferredUniversalCallbacks.ToArray();
                while (deferredRouteCallbacks.Count > 0) { deferredRouteCallbacks.Dequeue()(); }
                while (deferredMethodCallbacks.Count > 0) { deferredMethodCallbacks.Dequeue()(); }
            }

            private static Response Resolve(RouteCallback callback, ContextWrapper cw)
            {
                Response result = callback(new Request(cw));
                result.Merge(cw);
                return (result is PassThrough) ? null : result;
            }

            private static string RouteKey(string method, string route) => String.Join(null, method, route).ToUpper();

            private static void RouteAddingExceptions(string method, string route)
            {
                InitExceptions();
                if (!isValid(method) || !isValid(route)) { throw new ArgumentException("Routes and methods cannot be empty or contain spaces."); }
                bool isValid(string str) => str == null || str != string.Empty && !str.Contains(' ');
            }
        }
    }
}
