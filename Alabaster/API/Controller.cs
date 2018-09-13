using System;
using System.Collections.Generic;

namespace Alabaster
{
    using RouteCallback_A = Func<Request, Response>;
    using RouteCallback_B = Action<Request>;
    using RouteCallback_C = Func<Response>;
    using RouteCallback_D = Action;

    public readonly struct Controller
    {
        public readonly string Route;
        public readonly string Method;
        public readonly RouteCallback_A Callback;
        public Controller(string method, string route, RouteCallback_A callback)
        {
            this.Route = route;
            this.Method = method;
            this.Callback = callback;
        }
        public Controller(HTTPMethod method, string route, RouteCallback_A callback) :                              this(method.ToString(), route, callback) { }
        public Controller(string method, string route, RouteCallback_B callback) :                                  this(method, route, RouteCallback.Convert(callback)) { }
        public Controller(string method, string route, RouteCallback_C callback) :                                  this(method, route, RouteCallback.Convert(callback)) { }
        public Controller(string method, string route, RouteCallback_D callback) :                                  this(method, route, RouteCallback.Convert(callback)) { }
        public Controller(string method, string route, Response res) :                                              this(method, route, RouteCallback.ResponseShortcut(res)) { }
        public Controller(string method, RoutePatternMatch route, RouteCallback_A callback) :                       this(method, null, route.CreateCallback(callback)) { }
        public Controller(string method, RoutePatternMatch route, RouteCallback_B callback) :                       this(method, null, route.CreateCallback(callback)) { }
        public Controller(string method, RoutePatternMatch route, RouteCallback_C callback) :                       this(method, null, route.CreateCallback(callback)) { }
        public Controller(string method, RoutePatternMatch route, RouteCallback_D callback) :                       this(method, null, route.CreateCallback(callback)) { }
        public Controller(string method, RoutePatternMatch route, Response res) :                                   this(method, null, route.CreateCallback(res)) { }
        public Controller(HTTPMethod method, string route, RouteCallback_B callback) :                              this(method, route, RouteCallback.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, RouteCallback_C callback) :                              this(method, route, RouteCallback.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, RouteCallback_D callback) :                              this(method, route, RouteCallback.Convert(callback)) { }
        public Controller(HTTPMethod method, string route, Response res) :                                          this(method, route, RouteCallback.ResponseShortcut(res)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, RouteCallback_A callback) :                   this(method.ToString(), null, route.CreateCallback(callback)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, RouteCallback_B callback) :                   this(method.ToString(), null, route.CreateCallback(callback)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, RouteCallback_C callback) :                   this(method.ToString(), null, route.CreateCallback(callback)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, RouteCallback_D callback) :                   this(method.ToString(), null, route.CreateCallback(callback)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, Response res) :                               this(method.ToString(), null, route.CreateCallback(res)) { }
        public static implicit operator Controller((string m, string r, RouteCallback_A c) args) =>                 new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((string m, string r, RouteCallback_B c) args) =>                 new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((string m, string r, RouteCallback_C c) args) =>                 new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((string m, string r, RouteCallback_D c) args) =>                 new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((string m, string r, Response res) args) =>                      new Controller(args.m, args.r, args.res);
        public static implicit operator Controller((string m, RoutePatternMatch r, RouteCallback_A c) args) =>      new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((string m, RoutePatternMatch r, RouteCallback_B c) args) =>      new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((string m, RoutePatternMatch r, RouteCallback_C c) args) =>      new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((string m, RoutePatternMatch r, RouteCallback_D c) args) =>      new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((string m, RoutePatternMatch r, Response res) args) =>           new Controller(args.m, args.r, args.res);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_A c) args) =>             new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_B c) args) =>             new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_C c) args) =>             new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, RouteCallback_D c) args) =>             new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, string r, Response res) args) =>                  new Controller(args.m, args.r, args.res);
        public static implicit operator Controller((HTTPMethod m, RoutePatternMatch r, RouteCallback_A c) args) =>  new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, RoutePatternMatch r, RouteCallback_B c) args) =>  new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, RoutePatternMatch r, RouteCallback_C c) args) =>  new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, RoutePatternMatch r, RouteCallback_D c) args) =>  new Controller(args.m, args.r, args.c);
        public static implicit operator Controller((HTTPMethod m, RoutePatternMatch r, Response res) args) =>       new Controller(args.m, args.r, args.res);
        public static implicit operator Controller((string r, RouteCallback_A c) args) =>                           (null, args.r, args.c);
        public static implicit operator Controller((string r, RouteCallback_B c) args) =>                           (null, args.r, args.c);
        public static implicit operator Controller((string r, RouteCallback_C c) args) =>                           (null, args.r, args.c);
        public static implicit operator Controller((string r, RouteCallback_D c) args) =>                           (null, args.r, args.c);
        public static implicit operator Controller((string r, Response res) args) =>                                (null, args.r, args.res);
        public static implicit operator Controller((RoutePatternMatch r, RouteCallback_A c) args) =>                (null, args.r, args.c);
        public static implicit operator Controller((RoutePatternMatch r, RouteCallback_B c) args) =>                (null, args.r, args.c);
        public static implicit operator Controller((RoutePatternMatch r, RouteCallback_C c) args) =>                (null, args.r, args.c);
        public static implicit operator Controller((RoutePatternMatch r, RouteCallback_D c) args) =>                (null, args.r, args.c);
        public static implicit operator Controller((RoutePatternMatch r, Response res) args) =>                     (null, args.r, args.res);
        public static implicit operator Controller(RouteCallback_A c) =>                                            (null, null, c);
        public static implicit operator Controller(RouteCallback_B c) =>                                            (null, null, c);
        public static implicit operator Controller(RouteCallback_C c) =>                                            (null, null, c);
        public static implicit operator Controller(RouteCallback_D c) =>                                            (null, null, c);
        public static implicit operator Controller(Response res) =>                                                 (null, null, res);
    }
    
    public readonly struct PartialController
    {
        public readonly string Route;
        public readonly RouteCallback_A Callback;
        public PartialController(string route, RouteCallback_A callback)
        {
            this.Route = route;
            this.Callback = callback;
        }
        public static implicit operator PartialController((string r, RouteCallback_A c) args) => new PartialController(args.r, args.c);
        public static implicit operator PartialController((string r, RouteCallback_B c) args) => (args.r, RouteCallback.Convert(args.c));
        public static implicit operator PartialController((string r, RouteCallback_C c) args) => (args.r, RouteCallback.Convert(args.c));
        public static implicit operator PartialController((string r, RouteCallback_D c) args) => (args.r, RouteCallback.Convert(args.c));
        public static implicit operator PartialController((string r, Response res) args) => (args.r, RouteCallback.ResponseShortcut(args.res));
        public static implicit operator PartialController(Controller c) => (c.Route, c.Callback);
    }


}