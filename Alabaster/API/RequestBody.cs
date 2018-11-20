using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Net;

namespace Alabaster
{
    public readonly struct RequestBody
    {
        private readonly HttpListenerRequest req;
        public Stream InputStream => req.InputStream;

        public string AsString() => this.AsStringAsync().Result;
        public byte[] AsBuffer() => this.AsBufferAsync().Result;
        public async Task<string> AsStringAsync(int maximumSize = 104857600) => Encoding.UTF8.GetString( await AsBufferAsync(maximumSize) );
        public async Task<byte[]> AsBufferAsync(int maximumSize = 104857600)
        {
            int size = (int)Util.Clamp(this.req.ContentLength64, 0, maximumSize);
            byte[] buffer = new byte[size];
            await this.InputStream.ReadAsync(buffer, 0, size);
            return buffer;
        }

        internal RequestBody(HttpListenerRequest req)
        {
            this.req = req;
        }

        public override string ToString() => this.AsString();        
    }
}
