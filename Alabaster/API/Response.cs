using System;
using System.Collections;
using System.Net;
using System.Text;

namespace Alabaster
{
    public abstract partial class Response
    {
        protected byte[] data;
        public virtual byte[] Body { get => this.data; }
        public string ContentType;
        public Encoding ContentEncoding;
        public WebHeaderCollection Headers;
        public string StatusDescription;
        public CookieCollection Cookies = new CookieCollection();
        private bool? _KeepAlive;
        private int? _StatusCode;
        
        public bool KeepAlive
        {
            get => this._KeepAlive ?? false;
            set => this._KeepAlive = value;
        }

        public int StatusCode
        {
            get => this._StatusCode ?? 200;
            set => this._StatusCode = value;
        }
        
        public Response SetStatusCode(int statusCode)
        {
            this.StatusCode = statusCode;
            return this;
        }

        public Response AddSession(Session session)
        {
            Server.Get("", (req) => {

                return new DataResponse(null);
            });
            Cookie cookie = new Cookie(Server.Config.ServerID, session.id.ToString());
            cookie.HttpOnly = true;           
            this.Cookies.Add(cookie);
            return this;
        }

        internal void Merge(ContextWrapper cw)
        {
            HttpListenerResponse res = cw.Context.Response;
            res.ContentType = this.ContentType ?? res.ContentType;
            res.ContentEncoding = this.ContentEncoding ?? res.ContentEncoding;
            res.Headers = this.Headers ?? res.Headers;
            res.StatusDescription = this.StatusDescription ?? res.StatusDescription;
            res.KeepAlive = this._KeepAlive ?? res.KeepAlive;
            res.StatusCode = this._StatusCode ?? res.StatusCode;
            res.Cookies = mergeCookies(this.Cookies, res.Cookies, cw.Context.Request.Cookies);
            cw.Context.Request.Cookies.Add(res.Cookies);
            cw.ResponseBody = this.Body;

            CookieCollection mergeCookies(params CookieCollection[] cookies)
            {
                CookieCollection result = new CookieCollection();
                foreach (CookieCollection cc in cookies)
                {
                    foreach(Cookie cookie in cc)
                    {
                        if(result[cookie.Name] == null) { result.Add(cookie); }
                    }
                }
                return result;
            }
        }

        internal virtual void Finish(ContextWrapper cw)
        {
            HttpListenerResponse res = cw.Context.Response;
            byte[] data = cw.ResponseBody;
            res.ContentLength64 = data.Length;
            res.OutputStream.Write(data, 0, data.Length);
            res.OutputStream.Close();
        }
    }

    public sealed class RedirectResponse : Response
    {
        private string redirectRoute;
        public RedirectResponse(string route, int statusCode = 302)
        {
            this.redirectRoute = route;
            this.StatusCode = Util.Clamp(statusCode, 300, 399);
        }
        internal override void Finish(ContextWrapper cw)
        {
            cw.Context.Response.Redirect(redirectRoute);
            base.Finish(cw);
        }
    }

    public sealed class StringResponse : Response
    {
        public StringResponse(string response, int status = 200)
        {
            this.data = Encoding.UTF8.GetBytes(response);
            this.StatusCode = status;
        }
        public static implicit operator StringResponse(string str) => new StringResponse(str);        
    }

    public sealed class DataResponse : Response
    {
        public DataResponse(byte[] data) : this(data, (data == null) ? 501 : 200) { }
        public DataResponse(byte[] data, int status)
        {
            this.data = data;
            this.StatusCode = status;
        }
        public static implicit operator DataResponse(byte[] bytes) => new DataResponse(bytes);
        public static implicit operator DataResponse(FileIO.FileData file) => new DataResponse(file.Data);
    }

    public sealed class FileResponse : Response
    {
        private string fileName;
        private string baseDirectory;
        public override byte[] Body
        {
            get
            {
                if(this.data == null) { this.data = FileIO.GetFile(fileName, baseDirectory).Data; }
                return this.data;
            }
        }

        public FileResponse(string fileName) : this(fileName, Server.Config.StaticFilesBaseDirectory) { }

        public FileResponse(string fileName, string baseDirectory)
        {
            this.data = null;
            this.fileName = fileName;
            this.baseDirectory = baseDirectory;
            this.StatusCode = (this.data == null) ? 404 : 200;
        }
    }

    public sealed class EmptyResponse : Response
    {
        public EmptyResponse(int status)
        {
            this.StatusCode = status;
            this.data = null;
        }
    }

    public sealed class PassThrough : Response
    {
        public PassThrough() : this(null, 200) { }
        public PassThrough(byte[] data) : this(data, (data == null) ? 404 : 200) { }
        public PassThrough(byte[] data, int status)
        {
            this.data = data;
            this.StatusCode = status;
        }
    }
}