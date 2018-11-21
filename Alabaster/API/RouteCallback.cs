using System;

namespace Alabaster
{
    using RouteCallback_A = Func<Request, Response>;
    using RouteCallback_B = Action<Request>;
    using RouteCallback_C = Func<Response>;
    using RouteCallback_D = Action;

    internal readonly struct RouteCallback
    {
        internal readonly RouteCallback_A Callback;
        internal readonly int Priority;
        internal RouteCallback(RouteCallback_A cb, int priority = 0)
        {
            this.Callback = cb;
            this.Priority = priority;
        }
        public static implicit operator RouteCallback(RouteCallback_A cb) => new RouteCallback(cb);
        public static implicit operator RouteCallback(RouteCallback_B cb) => Convert(cb);
        public static implicit operator RouteCallback(RouteCallback_D cb) => Convert(cb);
        public static implicit operator RouteCallback(RouteCallback_C cb) => Convert(cb);
        public static implicit operator RouteCallback(Response res) => ResponseShortcut(res);

        internal static RouteCallback_A Convert(RouteCallback_B callback) => (Request req) => { callback(req); return PassThrough.Default; };
        internal static RouteCallback_A Convert(RouteCallback_C callback) => (Request req) => callback();
        internal static RouteCallback_A Convert(RouteCallback_D callback) => (Request req) => { callback(); return PassThrough.Default; };
        internal static RouteCallback_A ResponseShortcut(Response res) => (Request req) => res;
    }
}
