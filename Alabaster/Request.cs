using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace Alabaster
{
    public sealed class Request
    {
        internal ContextWrapper cw;
        internal HttpListenerRequest req { get => this.cw.Context.Request; }
        internal HttpListenerResponse res { get => this.cw.Context.Response; }
        internal Request(ContextWrapper cw) => this.cw = cw;
        private Session[] sessions;

        public Session[] Sessions
        {
            get
            {
                if(this.sessions == null) { ParseSessions(); }
                return this.sessions;
            }
        }
                
        private Request(){}

        private void ParseSessions()
        {
            Queue<Session> sq = new Queue<Session>();
            foreach (Cookie cookie in this.Cookies)
            {
                if (!Int64.TryParse(cookie.Name, out Int64 id)) { continue; }
                Int32.TryParse(cookie.Value, out Int32 key);
                Session session = Session.GetSession(id, key);
                if (session != null) { sq.Enqueue(session); }
            }
            this.sessions = sq.ToArray();
        }

        public int ClientCertificateError { get => this.req.ClientCertificateError; }
        public long ContentLength64 { get => this.req.ContentLength64; }
        public string[] UserLanguages { get => this.req.UserLanguages; }
        public string[] AcceptTypes { get => this.req.AcceptTypes; }
        public string UserHostName { get => this.req.UserHostName; }
        public string UserHostAddress { get => this.req.UserHostAddress; }
        public string UserAgent { get => this.req.UserAgent; }
        public string ServiceName { get => this.req.ServiceName; }
        public string RawUrl { get => this.req.RawUrl; }
        public string HttpMethod { get => this.req.HttpMethod; }
        public string ContentType { get => this.req.ContentType; }
        public bool IsSecureConnection { get => this.req.IsSecureConnection; }
        public bool IsLocal { get => this.req.IsLocal; }
        public bool IsAuthenticated { get => this.req.IsAuthenticated; }
        public bool IsWebSocketRequest { get => this.req.IsWebSocketRequest; }
        public bool KeepAlive { get => this.req.KeepAlive; }
        public bool HasEntityBody { get => this.req.HasEntityBody; }
        public Uri UrlReferrer { get => this.req.UrlReferrer; }
        public Uri Url { get => this.req.Url; }
        public IPEndPoint RemoteEndPoint { get => this.req.RemoteEndPoint; }
        public IPEndPoint LocalEndPoint { get => this.req.LocalEndPoint; }
        public Stream InputStream { get => this.req.InputStream; }
        public NameValueCollection Headers { get => this.req.Headers; }
        public Encoding ContentEncoding { get => this.req.ContentEncoding; }
        public Guid RequestTraceIdentifier { get => this.req.RequestTraceIdentifier; }
        public NameValueCollection QueryString { get => this.req.QueryString; }
        public Version ProtocolVersion { get => this.req.ProtocolVersion; }
        public CookieCollection Cookies { get => this.req.Cookies; }
        public TransportContext TransportContext { get => this.req.TransportContext; }
    }
}