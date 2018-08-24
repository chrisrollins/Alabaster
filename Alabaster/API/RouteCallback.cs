using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public delegate Response RouteCallback_A(Request req);
    public delegate void RouteCallback_B(Request req);
    public delegate Response RouteCallback_C();
    public delegate void RouteCallback_D();

    internal struct RouteCallback
    {
        internal RouteCallback_A Callback;
        internal RouteCallback(RouteCallback_A cb) => this.Callback = cb;
        public static implicit operator RouteCallback(RouteCallback_A cb) => new RouteCallback(cb);
        public static implicit operator RouteCallback(RouteCallback_B cb) => Convert(cb);
        public static implicit operator RouteCallback(RouteCallback_D cb) => Convert(cb);
        public static implicit operator RouteCallback(RouteCallback_C cb) => Convert(cb);
        public static implicit operator RouteCallback(Response res) => ResponseShortcut(res);

        internal static RouteCallback_A Convert(RouteCallback_B callback) => (Request req) => { callback(req); return new PassThrough(); };
        internal static RouteCallback_A Convert(RouteCallback_C callback) => (Request req) => callback();
        internal static RouteCallback_A Convert(RouteCallback_D callback) => (Request req) => { callback(); return new PassThrough(); };
        internal static RouteCallback_A ResponseShortcut(Response res) => (Request req) => res;
    }
}
