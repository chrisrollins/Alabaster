using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public partial struct Request
    {
        public RequestDiagnostics Diagnostics { get => new RequestDiagnostics(this); }

        public class RequestDiagnostics
        {
            internal RequestDiagnostics(Request req)
            {
                (RouteCallback_A matchedRoute, RouteCallback_A matchedMethod) = Server.GetMatchingRoutes(req);
                if (matchedRoute != null) { MatchingControllers.Add(new ControllerInfo(req.HttpMethod, req.Url.AbsolutePath)); }
                if (matchedMethod != null) { MatchingControllers.Add(new ControllerInfo(req.HttpMethod, null)); }
            }

            public readonly List<ControllerInfo> MatchingControllers = new List<ControllerInfo>(2);
        }

        public struct ControllerInfo
        {
            public ControllerInfo(string method, string url)
            {
                this.Method = method;
                this.URL = url;
            }
            string Method;
            string URL;            
        }
    }

    public partial class Server
    {
        internal static (RouteCallback_A, RouteCallback_A) GetMatchingRoutes(Request req)
        {
            Routing.routeCallbacks.TryGetValue(req.cw, out RouteCallback_A rc);
            Routing.methodCallbacks.TryGetValue((MethodArg)req.HttpMethod, out RouteCallback_A mc);
            return (rc, mc);
        }
    }
}
