using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public struct Controller
    {
        public string Route;
        public string Method;
        public RouteCallback_A Callback;
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
        public Controller(HTTPMethod method, RoutePatternMatch route, RouteCallback_A callback) :                   this(method, null, route.CreateCallback(callback)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, RouteCallback_B callback) :                   this(method, null, route.CreateCallback(callback)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, RouteCallback_C callback) :                   this(method, null, route.CreateCallback(callback)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, RouteCallback_D callback) :                   this(method, null, route.CreateCallback(callback)) { }
        public Controller(HTTPMethod method, RoutePatternMatch route, Response res) :                               this(method, null, route.CreateCallback(res)) { }
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
    }
}
