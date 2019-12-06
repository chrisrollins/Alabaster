using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Alabaster
{
    using RouteCallback_A = Func<Request, Response>;
    using RouteCallback_B = Action<Request>;
    using RouteCallback_C = Func<Response>;
    using RouteCallback_D = Action;

    public static partial class Server
    {
        public static void Routes(params Controller[] controllers) => Array.ForEach(controllers, (Controller c) => Routing.AddHandler((MethodArg)c.Method, (RouteArg)c.Route, c.Callback));
        public static void Get(params PartialController[] controllers) => Array.ForEach(controllers, (PartialController c) => Routing.AddHandler(HTTPMethod.GET, (RouteArg)c.Route, c.Callback));
        public static void Post(params PartialController[] controllers) => Array.ForEach(controllers, (PartialController c) => Routing.AddHandler(HTTPMethod.POST, (RouteArg)c.Route, c.Callback));
        public static void Patch(params PartialController[] controllers) => Array.ForEach(controllers, (PartialController c) => Routing.AddHandler(HTTPMethod.PATCH, (RouteArg)c.Route, c.Callback));
        public static void Put(params PartialController[] controllers) => Array.ForEach(controllers, (PartialController c) => Routing.AddHandler(HTTPMethod.PUT, (RouteArg)c.Route, c.Callback));
        public static void Delete(params PartialController[] controllers) => Array.ForEach(controllers, (PartialController c) => Routing.AddHandler(HTTPMethod.DELETE, (RouteArg)c.Route, c.Callback));
        public static void Get(string route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.GET, (RouteArg)route, callback);
        public static void Get(string route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.GET, (RouteArg)route, callback);
        public static void Get(string route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.GET, (RouteArg)route, callback);
        public static void Get(string route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.GET, (RouteArg)route, callback);
        public static void Get(string route, Response res) => Routing.AddHandler(HTTPMethod.GET, (RouteArg)route, res);
        public static void Post(string route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.POST, (RouteArg)route, callback);
        public static void Post(string route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.POST, (RouteArg)route, callback);
        public static void Post(string route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.POST, (RouteArg)route, callback);
        public static void Post(string route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.POST, (RouteArg)route, callback);
        public static void Post(string route, Response res) => Routing.AddHandler(HTTPMethod.POST, (RouteArg)route, res);
        public static void Patch(string route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.PATCH, (RouteArg)route, callback);
        public static void Patch(string route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.PATCH, (RouteArg)route, callback);
        public static void Patch(string route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.PATCH, (RouteArg)route, callback);
        public static void Patch(string route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.PATCH, (RouteArg)route, callback);
        public static void Patch(string route, Response res) => Routing.AddHandler(HTTPMethod.PATCH, (RouteArg)route, res);
        public static void Put(string route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.PUT, (RouteArg)route, callback);
        public static void Put(string route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.PUT, (RouteArg)route, callback);
        public static void Put(string route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.PUT, (RouteArg)route, callback);
        public static void Put(string route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.PUT, (RouteArg)route, callback);
        public static void Put(string route, Response res) => Routing.AddHandler(HTTPMethod.PUT, (RouteArg)route, res);
        public static void Delete(string route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.DELETE, (RouteArg)route, callback);
        public static void Delete(string route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.DELETE, (RouteArg)route, callback);
        public static void Delete(string route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.DELETE, (RouteArg)route, callback);
        public static void Delete(string route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.DELETE, (RouteArg)route, callback);
        public static void Delete(string route, Response res) => Routing.AddHandler(HTTPMethod.DELETE, (RouteArg)route, res);
        public static void Route(string route, RouteCallback_A callback) => Routing.AddHandler((RouteArg)route, callback);
        public static void Route(string route, RouteCallback_B callback) => Routing.AddHandler((RouteArg)route, callback);
        public static void Route(string route, RouteCallback_C callback) => Routing.AddHandler((RouteArg)route, callback);
        public static void Route(string route, RouteCallback_D callback) => Routing.AddHandler((RouteArg)route, callback);
        public static void Route(string route, Response res) => Routing.AddHandler((RouteArg)route, res);
        public static void Route(string method, string route, RouteCallback_A callback) => Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, RouteCallback_B callback) => Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, RouteCallback_C callback) => Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, RouteCallback_D callback) => Routing.AddHandler((MethodArg)method, (RouteArg)route, callback);
        public static void Route(string method, string route, Response res) => Routing.AddHandler((MethodArg)method, (RouteArg)route, res);
        public static void Route(HTTPMethod method, string route, RouteCallback_B callback) => Routing.AddHandler(method, (RouteArg)route, callback);
        public static void Route(HTTPMethod method, string route, RouteCallback_C callback) => Routing.AddHandler(method, (RouteArg)route, callback);
        public static void Route(HTTPMethod method, string route, RouteCallback_D callback) => Routing.AddHandler(method, (RouteArg)route, callback);
        public static void Route(HTTPMethod method, string route, RouteCallback_A callback) => Routing.AddHandler(method, (RouteArg)route, callback);
        public static void Route(HTTPMethod method, string route, Response res) => Routing.AddHandler(method, (RouteArg)route, res);
        public static void All(HTTPMethod method, RouteCallback_A callback) => Routing.AddHandler(method, callback);
        public static void All(HTTPMethod method, RouteCallback_B callback) => Routing.AddHandler(method, callback);
        public static void All(HTTPMethod method, RouteCallback_C callback) => Routing.AddHandler(method, callback);
        public static void All(HTTPMethod method, RouteCallback_D callback) => Routing.AddHandler(method, callback);
        public static void All(HTTPMethod method, Response res) => Routing.AddHandler(method, res);
        public static void All(RouteCallback_A callback) => Routing.AddHandler(callback);
        public static void All(RouteCallback_B callback) => Routing.AddHandler(callback);
        public static void All(RouteCallback_C callback) => Routing.AddHandler(callback);
        public static void All(RouteCallback_D callback) => Routing.AddHandler(callback);
        public static void All(Response res) => Routing.AddHandler(res);
        public static void Get(RoutePatternMatch route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.GET, route, callback);
        public static void Get(RoutePatternMatch route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.GET, route, callback);
        public static void Get(RoutePatternMatch route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.GET, route, callback);
        public static void Get(RoutePatternMatch route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.GET, route, callback);
        public static void Get(RoutePatternMatch route, Response res) => Routing.AddHandler(HTTPMethod.GET, route, res);
        public static void Post(RoutePatternMatch route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.POST, route, callback);
        public static void Post(RoutePatternMatch route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.POST, route, callback);
        public static void Post(RoutePatternMatch route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.POST, route, callback);
        public static void Post(RoutePatternMatch route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.POST, route, callback);
        public static void Post(RoutePatternMatch route, Response res) => Routing.AddHandler(HTTPMethod.POST, route, res);
        public static void Patch(RoutePatternMatch route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.PATCH, route, callback);
        public static void Patch(RoutePatternMatch route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.PATCH, route, callback);
        public static void Patch(RoutePatternMatch route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.PATCH, route, callback);
        public static void Patch(RoutePatternMatch route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.PATCH, route, callback);
        public static void Patch(RoutePatternMatch route, Response res) => Routing.AddHandler(HTTPMethod.PATCH, route, res);
        public static void Put(RoutePatternMatch route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.PUT, route, callback);
        public static void Put(RoutePatternMatch route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.PUT, route, callback);
        public static void Put(RoutePatternMatch route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.PUT, route, callback);
        public static void Put(RoutePatternMatch route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.PUT, route, callback);
        public static void Put(RoutePatternMatch route, Response res) => Routing.AddHandler(HTTPMethod.PUT, route, res);
        public static void Delete(RoutePatternMatch route, RouteCallback_A callback) => Routing.AddHandler(HTTPMethod.DELETE, route, callback);
        public static void Delete(RoutePatternMatch route, RouteCallback_B callback) => Routing.AddHandler(HTTPMethod.DELETE, route, callback);
        public static void Delete(RoutePatternMatch route, RouteCallback_C callback) => Routing.AddHandler(HTTPMethod.DELETE, route, callback);
        public static void Delete(RoutePatternMatch route, RouteCallback_D callback) => Routing.AddHandler(HTTPMethod.DELETE, route, callback);
        public static void Delete(RoutePatternMatch route, Response res) => Routing.AddHandler(HTTPMethod.DELETE, route, res);
        public static void Route(RoutePatternMatch route, RouteCallback_A callback) => Routing.AddHandler(route, callback);
        public static void Route(RoutePatternMatch route, RouteCallback_B callback) => Routing.AddHandler(route, callback);
        public static void Route(RoutePatternMatch route, RouteCallback_C callback) => Routing.AddHandler(route, callback);
        public static void Route(RoutePatternMatch route, RouteCallback_D callback) => Routing.AddHandler(route, callback);
        public static void Route(RoutePatternMatch route, Response res) => Routing.AddHandler(route, res);
        public static void Route(string method, RoutePatternMatch route, RouteCallback_A callback) => Routing.AddHandler((MethodArg)method, route, callback);
        public static void Route(string method, RoutePatternMatch route, RouteCallback_B callback) => Routing.AddHandler((MethodArg)method, route, callback);
        public static void Route(string method, RoutePatternMatch route, RouteCallback_C callback) => Routing.AddHandler((MethodArg)method, route, callback);
        public static void Route(string method, RoutePatternMatch route, RouteCallback_D callback) => Routing.AddHandler((MethodArg)method, route, callback);
        public static void Route(string method, RoutePatternMatch route, Response res) => Routing.AddHandler((MethodArg)method, route, res);
        public static void Route(HTTPMethod method, RoutePatternMatch route, RouteCallback_A callback) => Routing.AddHandler(method, route, callback);
        public static void Route(HTTPMethod method, RoutePatternMatch route, RouteCallback_B callback) => Routing.AddHandler(method, route, callback);
        public static void Route(HTTPMethod method, RoutePatternMatch route, RouteCallback_C callback) => Routing.AddHandler(method, route, callback);
        public static void Route(HTTPMethod method, RoutePatternMatch route, RouteCallback_D callback) => Routing.AddHandler(method, route, callback);
        public static void Route(HTTPMethod method, RoutePatternMatch route, Response res) => Routing.AddHandler(method, route, res);

        public delegate void PrefixedRouteAdder(params PartialController[] routes);

        public static PrefixedRouteAdder Get(string prefix) => AddControllersWithPrefix(HTTPMethod.GET, (RouteArg)prefix);
        public static PrefixedRouteAdder Post(string prefix) => AddControllersWithPrefix(HTTPMethod.POST, (RouteArg)prefix);
        public static PrefixedRouteAdder Patch(string prefix) => AddControllersWithPrefix(HTTPMethod.PATCH, (RouteArg)prefix);
        public static PrefixedRouteAdder Put(string prefix) => AddControllersWithPrefix(HTTPMethod.PUT, (RouteArg)prefix);
        public static PrefixedRouteAdder Delete(string prefix) => AddControllersWithPrefix(HTTPMethod.DELETE, (RouteArg)prefix);
        private static PrefixedRouteAdder AddControllersWithPrefix(MethodArg method, RouteArg prefix)
        {
            void Adder(params PartialController[] controllers)
            {
                Controller[] prefixedControllers = new Controller[controllers.Length];
                for(int i = 0; i < controllers.Length; i++)
                {
                    PartialController beforePrefix = controllers[i];
                    if(beforePrefix.Route == null) { throw new ArgumentException("Cannot use a RoutePatternMatch route with the prefix adder."); }
                    prefixedControllers[i] = (method.Value, prefix.Value + beforePrefix.Route.TrimStart('/', '\\'), beforePrefix.Callback);
                }
                Routes(prefixedControllers);
            }
            return Adder;
        }
    }

    internal struct MethodArg
    {
        public readonly string Value;
        static readonly string[] stdMethods = Enum.GetNames(typeof(HTTPMethod));
        public MethodArg(string val, bool validate = true)
        {
            if (val == null) { this.Value = null; }
            else
            {
                if (validate && !Server.Config.EnableCustomHTTPMethods && !stdMethods.Contains(val.ToUpper())) { throw new ArgumentException("Non-standard HTTP method: \"" + val + "\". Enable non-standard HTTP methods to use a custom method by setting Server.Config.EnableCustomHTTPMethods to true."); }
                this.Value = (val == null) ? null : val.ToUpper();
            }
        }
        public MethodArg(HTTPMethod method) => this.Value = method.ToString();
        public static explicit operator MethodArg(string method) => new MethodArg(method);
        public static implicit operator MethodArg(HTTPMethod method) => new MethodArg(method);
    }

    internal struct RouteArg
    {
        public readonly string Value;
        public RouteArg(string route, bool validate = true)
        {
            route = route?.Replace('\\', '/');
            if (validate && route != null) { RouteValidator.EnforceValidation(route); }
            this.Value = (route == null) ? null : string.Join(null, (route.First() != '/') ? "/" : "", route, (route.Last() != '/') ? "/" : "");
        }
        public static explicit operator RouteArg(string s) => new RouteArg(s);
    }

    internal static class Routing
    {
        private static Queue<(MethodArg method, RouteArg route, RouteCallback callback)> currentHandlerGroup = new Queue<(MethodArg, RouteArg, RouteCallback)>(100);
        private enum HandlerType : byte { Universal, URL, URL_AllMethods, StandardMethod, CustomMethod }
        private static HandlerType? lastHandlerType = null;
        private static int? lastPriority = null;

        internal static void AddHandler(RoutePatternMatch rp, RouteCallback rc) => AddHandler((MethodArg)null, rp, rc);
        internal static void AddHandler(MethodArg method, RoutePatternMatch rp, RouteCallback rc) => AddHandler(method, rp.CreateCallback(rc.Callback));
        internal static void AddHandler(MethodArg method, RouteCallback rc) => AddHandler(method, (RouteArg)null, rc);
        internal static void AddHandler(RouteArg route, RouteCallback rc) => AddHandler((MethodArg)null, route, rc);
        internal static void AddHandler(RouteCallback rc) => AddHandler((MethodArg)null, (RouteArg)null, rc);
        internal static void AddHandler(MethodArg method, RouteArg route, RouteCallback rc)
        {
            InternalQueueManager.SetupQueue.Run(() => 
            {
                Util.InitExceptions();
                AddHandlerInternal(method, route, rc);
            });
        }

        internal static void AddHandlerInternal(MethodArg method, RouteArg route, RouteCallback_A rc) => AddHandlerInternal(method, route, new RouteCallback(rc));
        internal static void AddHandlerInternal(MethodArg method, RouteArg route, RouteCallback rc)
        {
            RouteAddingExceptions(method.Value, route.Value, rc.Callback);
            HandlerType currentHandlerType = GetHandlerType();
            int currentPriority = rc.Priority;
            if (currentHandlerType != lastHandlerType || currentPriority != lastPriority) { ProcessHandlerQueue(lastPriority ?? 0); }
            currentHandlerGroup.Enqueue((method, route, rc));
            lastHandlerType = currentHandlerType;
            lastPriority = currentPriority;

            HandlerType GetHandlerType()
            {
                if (route.Value == null && method.Value == null) { return HandlerType.Universal; }
                if (route.Value != null) { return (method.Value == null) ? HandlerType.URL_AllMethods : HandlerType.URL; }
                if (Array.IndexOf(Enum.GetNames(typeof(HTTPMethod)), method.Value) != -1) { return HandlerType.StandardMethod; }
                return HandlerType.CustomMethod;
            }
        }

        private static void ProcessHandlerQueue(int priority)
        {
            if (currentHandlerGroup.Count == 0) { return; }

            switch(lastHandlerType)
            {
                case HandlerType.Universal:
                    Universal();
                    break;
                case HandlerType.StandardMethod:
                case HandlerType.CustomMethod:
                    Method();
                    break;
                case HandlerType.URL:
                    URL();
                    break;
                case HandlerType.URL_AllMethods:
                    URL_AllMethods();
                    break;
            }

            void Universal()
            {
                while (currentHandlerGroup.Count > 0)
                {
                    Handlers.Add(currentHandlerGroup.Dequeue().callback);
                }
            }

            void Method()
            {
                Dictionary<RoutingKey, RouteCallback_A> dict = new Dictionary<RoutingKey, RouteCallback_A>(Enum.GetValues(typeof(HTTPMethod)).Length);
                do
                {
                    (MethodArg m, RouteArg r, RouteCallback c) = currentHandlerGroup.Dequeue();
                    RoutingKey key = (MethodArg)m.Value;
                    dict[key] = (dict.TryGetValue(key, out RouteCallback_A existingCallback)) ? (Request req) => existingCallback(req) ?? c.Callback(req) : c.Callback;
                } while (currentHandlerGroup.Count > 0);
                Handlers.Add((Request req) => dict.TryGetValue((MethodArg)req.HttpMethod, out RouteCallback_A handler) ? handler(req) : PassThrough.Skip, priority);
            }

            void URL()
            {
                Dictionary<RoutingKey, RouteCallback_A> dict = new Dictionary<RoutingKey, RouteCallback_A>(currentHandlerGroup.Count);
                do
                {
                    (MethodArg m, RouteArg r, RouteCallback c) = currentHandlerGroup.Dequeue();
                    RoutingKey key = (m, r);
                    dict[key] = (dict.TryGetValue(key, out RouteCallback_A existingCallback)) ? (Request req) => existingCallback(req) ?? c.Callback(req) : c.Callback;
                } while (currentHandlerGroup.Count > 0);
                Handlers.Add((Request req) => (RouteValidator.IsValid(req.Route) && dict.TryGetValue(req.cw, out RouteCallback_A handler)) ? handler(req) : PassThrough.Skip, priority);
                
            }

            void URL_AllMethods()
            {
                Dictionary<RouteArg, RouteCallback_A> dict = new Dictionary<RouteArg, RouteCallback_A>(currentHandlerGroup.Count);
                do
                {
                    (_, RouteArg key, RouteCallback c) = currentHandlerGroup.Dequeue();
                    dict[key] = (dict.TryGetValue(key, out RouteCallback_A existingCallback)) ? (Request req) => existingCallback(req) ?? c.Callback(req) : c.Callback;
                } while (currentHandlerGroup.Count > 0);                    
                Handlers.Add((Request req) => (RouteValidator.IsValid(req.Route) && dict.TryGetValue((RouteArg)req.Route, out RouteCallback_A handler)) ? handler(req) : PassThrough.Skip, priority);
            }
        }

        internal static void Initialize()
        {
            AddHandlerInternal((MethodArg)null, (RouteArg)null, new RouteCallback( (Request req) => (Util.GetFileExtension(req.Route) != null) ? 404 : 400 ));
            ProcessHandlerQueue(lastPriority ?? 0);
            Handlers.Initialize();
        }

        internal static Response ResolveHandlers(ContextWrapper cw)
        {
            Response result = null;
            foreach(RouteCallback_A handler in Handlers.FinalizedHandlers)
            {
                result = handler(new Request(cw));
                if (!result.Skipped) { result.Merge(cw); }
                if (!(result is PassThrough)) { break; }
            }
            return (result is PassThrough) ? (result._StatusCode ?? 400) : result;
        }

        private static void RouteAddingExceptions(string method, string route, RouteCallback_A callback)
        {
            Util.InitExceptions();
            if (callback == null) { throw new ArgumentNullException("Callback cannot be null."); }
            if (!isValid(method) || !isValid(route)) { throw new ArgumentException("Routes and methods cannot be empty or contain spaces."); }
            bool isValid(string str) => str == null || str != string.Empty && !str.Contains(' ');
        }

        private static class Handlers
        {
            private static List<RouteCallback_A>[] handlers = new List<RouteCallback_A>[10];
            internal static RouteCallback_A[] FinalizedHandlers;

            internal static void Add(RouteCallback_A rc, int priority) => Add(new RouteCallback(rc, priority));
            internal static void Add(RouteCallback_A rc) => Add(new RouteCallback(rc));
            internal static void Add(RouteCallback rc)
            {
                RouteCallback_A finalHandler = rc.Callback;
                if(Server.Config.EnableRouteDiagnostics)
                {
                    finalHandler = (Request req) =>
                    {
                        req.Diagnostics.PushHandler(rc.Callback);
                        Response res = rc.Callback(req);
                        req.Diagnostics.PushResult(res);
                        return res;
                    };
                }
                int priority = rc.Priority;
                if(handlers[priority] == null) { handlers[priority] = new List<RouteCallback_A>(1024); }
                handlers[priority].Add(finalHandler);
            }

            internal static void Initialize()
            {
                List<RouteCallback_A> allHandlers = new List<RouteCallback_A>(1024);
                for (int i = handlers.Length - 1; i >= 0; i--)
                {
                    if(handlers[i] != null) { allHandlers.AddRange(handlers[i]); }
                }
                FinalizedHandlers = allHandlers.ToArray();
                handlers = null;
            }
        }
        
        internal struct RoutingKey
        {
            private (RouteArg route, MethodArg method) data;
            public override int GetHashCode() => data.GetHashCode();
            public RoutingKey(MethodArg method, RouteArg route) => this.data = (route, method);
            public RoutingKey(string method, string route) : this((MethodArg)method, (RouteArg)route) { }
            public RoutingKey(MethodArg method) : this(method, (RouteArg)null) { }
            public RoutingKey(RouteArg route) : this((MethodArg)null, route) { }
            public RoutingKey(ContextWrapper cw) : this(cw.Context.Request.HttpMethod, cw.Context.Request.Url.AbsolutePath) { }
            public static implicit operator RoutingKey((MethodArg m, RouteArg r) args) => new RoutingKey(args.m, args.r);
            public static implicit operator RoutingKey(MethodArg method) => new RoutingKey(method);
            public static implicit operator RoutingKey(RouteArg route) => new RoutingKey(route);
            public static implicit operator RoutingKey(ContextWrapper cw) => new RoutingKey(cw);
        }
    }
}