using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Alabaster
{
    internal sealed class ContextWrapper
    {
        internal HttpListenerContext Context;
        internal readonly RequestDiagnostics diagnostics;
        private byte[] data;
        internal byte[] ResponseBody
        {
            get => this.data ?? new byte[] { };
            set => this.data = value;
        }

        internal ContextWrapper(HttpListenerContext ctx)
        {
            this.Context = ctx;
            this.data = null;
            this.diagnostics = Server.Config.EnableRouteDiagnostics ? new RequestDiagnostics() : null;
        }

        internal string Route => this.Context.Request.Url.AbsolutePath;
        internal string HttpMethod => this.Context.Request.HttpMethod;
    }
}