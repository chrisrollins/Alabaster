﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alabaster
{
    public struct Request
    {        
        internal readonly ContextWrapper cw;
        internal HttpListenerRequest req { get => this.cw.Context.Request; }
        internal HttpListenerResponse res { get => this.cw.Context.Response; }
        private readonly SessionCollection sessions;        

        public Session[] Sessions { get => this.sessions.SessionList; }
        public string[] SessionNames { get => this.sessions.NameList; }
        public Session GetSession(string name) => this.sessions[name];
        public RequestDiagnostics Diagnostics
        {
            get
            {
                if(!Server.Config.EnableRouteDiagnostics) { throw new InvalidOperationException("Server.Config.EnableRouteDiagnostics must be true to use this feature."); }
                return this.cw.diagnostics;
            }
        }
        
        internal Request(ContextWrapper cw)
        {
            this.cw = cw;
            this.sessions = new SessionCollection(cw.Context.Request.Cookies);
            this.Parameters = new NameValueCollection(0);
            this.Body = new RequestBody(cw.Context.Request);
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
        public string Route => this.cw.Route;

        public readonly RequestBody Body;

        public NameValueCollection Parameters { get; internal set; }
        
        private struct SessionCollection
        {
            private ConcurrentDictionary<string, Session> _sessions;
            private CookieCollection cookies;
            private volatile int parsed;

            public SessionCollection(CookieCollection cookies)
            {
                this._sessions = new ConcurrentDictionary<string, Session>(Environment.ProcessorCount, 100);
                this.cookies = cookies;
                this.parsed = 0;
            }
            
            private ConcurrentDictionary<string, Session> sessions
            {
                get
                {
                    this.ParseSessions();
                    return this._sessions;
                }
            }

            public Session this[string name]
            {
                get
                {
                    sessions.TryGetValue(name, out Session session);
                    return session;
                }
            }

            public Session[] SessionList => this.sessions.Values.ToArray();
            public string[] NameList => this.sessions.Keys.ToArray();

            private void ParseSessions()
            {
                if (Interlocked.CompareExchange(ref parsed, 1, 0) == 1) { return; }
                this._sessions = new ConcurrentDictionary<string, Session>(Environment.ProcessorCount, 10);
                foreach (Cookie cookie in this.cookies)
                {
                    if (cookie.Name != Server.Config.ServerID) { continue; }
                    Session session = Session.GetSession(cookie.Value);
                    if (session != null) { this.sessions.TryAdd(session.name, session); }
                }
            }
        }
    }
}