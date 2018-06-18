using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Alabaster
{
    internal sealed class ContextWrapper
    {
        internal HttpListenerContext Context;
        private byte[] data;
        internal byte[] ResponseBody
        {
            get => this.data ?? new byte[] { };
            set => this.data = value;
        }

        internal ContextWrapper(HttpListenerContext ctx) => this.Context = ctx;
    }
}