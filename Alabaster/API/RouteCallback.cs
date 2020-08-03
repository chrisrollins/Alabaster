using System;
using System.Text;

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
        internal static RouteCallback_A ResponseShortcut(Response res)
        {
            switch (res)
            {
                case RedirectResponse r:
                    return (req) => new RedirectResponse((res as RedirectResponse).RedirectRoute, res.StatusCode);
                case StringResponse r:
                    return (req) => new StringResponse(Encoding.UTF8.GetString(res.Body), res.StatusCode);
                case DataResponse r:
                    return (req) => new DataResponse(res.Body, res.StatusCode);
                case FileResponse r:
                    return (req) => new FileResponse((res as FileResponse).FileName, (res as FileResponse).BaseDirectory);
                case EmptyResponse r:
                    return (req) => new EmptyResponse(res.StatusCode);
                case PassThrough r:
                    return (req) => new PassThrough(res.Body, res.StatusCode);
                case NoResponse r:
                    return (req) => new NoResponse();
                case WebSocketHandshake r:
                    throw new InvalidOperationException("An internal error has occurred in WebSocket initialization.");
                default:
                    throw new ArgumentException("Cannot pass a custom Response type without a handler callback.");
            }
        }
    }
}
