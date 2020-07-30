using System;
using System.Collections;
using System.Net;
using System.Text;

namespace Alabaster
{
    public abstract partial class Response
    {
        protected byte[] data;
        protected bool noResponse = false;
        public virtual byte[] Body { get => this.data; }
        public string ContentType;
        public Encoding ContentEncoding;
        public WebHeaderCollection Headers;
        public string StatusDescription;
        public CookieCollection Cookies = new CookieCollection();
        private bool? _KeepAlive;
        internal int? _StatusCode;
        private bool stale = false;
        private object staleLock = new object();
        internal bool Skipped = false;

        public bool KeepAlive
        {
            get => this._KeepAlive ?? false;
            set => this._KeepAlive = value;
        }

        public virtual int StatusCode
        {
            get => this._StatusCode ?? 200;
            set => this._StatusCode = value;
        }

        public Response SetStatusCode(HTTPStatus status) => SetStatusCode((Int32)status);
        public Response SetStatusCode(int status)
        {
            this.StatusCode = status;
            return this;
        }

        internal Response AddSession(Session session)
        {
            Cookie cookie = new Cookie(Server.Config.ServerID, session.id);
            cookie.HttpOnly = true;
            this.Cookies.Add(cookie);
            return this;
        }

        internal void Merge(ContextWrapper cw)
        {
            lock(staleLock)
            {
                if(this.stale) { throw new InvalidOperationException("Instance of Response must not be reused by multiple requests."); }
                this.stale = true;
            }
            HttpListenerResponse res = cw.Context.Response;
            res.ContentType = this.ContentType ?? res.ContentType;
            res.ContentEncoding = this.ContentEncoding ?? res.ContentEncoding;
            res.Headers = this.Headers ?? res.Headers;
            res.StatusCode = this._StatusCode ?? res.StatusCode;
            res.StatusDescription = this.StatusDescription ?? res.StatusDescription;
            res.KeepAlive = this._KeepAlive ?? res.KeepAlive;
            res.Cookies = mergeCookies(this.Cookies, res.Cookies, cw.Context.Request.Cookies);
            cw.Context.Request.Cookies.Add(res.Cookies);
            cw.Context.Response.Cookies = res.Cookies;
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
            if (this.noResponse == false)
            {
                string _ = res.StatusDescription;
                byte[] data = cw.ResponseBody;
                res.ContentLength64 = data.Length;
                res.OutputStream.Write(data, 0, data.Length);
                res.Close();
            }
            else
            {
                res.Abort();
            }
            this.AdditionalFinishTasks(new Request(cw), this);
        }
        
        public static implicit operator Response(FileIO.FileData file) => new FileResponse(file);
        public static implicit operator Response(byte[] bytes) => new DataResponse(bytes);
        public static implicit operator Response(byte b) => new DataResponse(new byte[] { b });
        public static implicit operator Response(string str) => new StringResponse(str);
        public static implicit operator Response((string str, int status) arg) => new StringResponse(arg.str, arg.status);
        public static implicit operator Response((int status, string str) arg) => new StringResponse(arg.str, arg.status);
        public static implicit operator Response(char c) => new StringResponse(c.ToString());
        public static implicit operator Response(HTTPStatus status) => new EmptyResponse(status);
        public static implicit operator Response(Int64 n) => new EmptyResponse((Int32)n);
        public static implicit operator Response(Int32 n) => new EmptyResponse(n);
        public static implicit operator Response(Int16 n) => new EmptyResponse(n);
        public static implicit operator Response(UInt64 n) => new EmptyResponse((Int32)n);
        public static implicit operator Response(UInt32 n) => new EmptyResponse((Int32)n);
        public static implicit operator Response(UInt16 n) => new EmptyResponse(n);
        public static implicit operator Response(Int64[] arr) => JoinArr(arr);
        public static implicit operator Response(Int32[] arr) => JoinArr(arr);
        public static implicit operator Response(Int16[] arr) => JoinArr(arr);
        public static implicit operator Response(UInt64[] arr) => JoinArr(arr);
        public static implicit operator Response(UInt32[] arr) => JoinArr(arr);
        public static implicit operator Response(UInt16[] arr) => JoinArr(arr);
        public static implicit operator Response(char[] arr) => JoinArr(arr);
        public static implicit operator Response(float[] arr) => JoinArr(arr);
        public static implicit operator Response(double[] arr) => JoinArr(arr);
        public static implicit operator Response(decimal[] arr) => JoinArr(arr);

        private static string JoinArr<T>(T[] arr) => string.Join(null, "[", string.Join(",", arr ?? new T[] { }), "]");
        
        public static Response Default => new EmptyResponse(HTTPStatus.BadRequest);
    }

    public sealed class RedirectResponse : Response
    {
        internal string RedirectRoute;
        public RedirectResponse(string route, HTTPStatus status = HTTPStatus.SeeOther) : this(route, (Int32)status) { }
        public RedirectResponse(string route, int status)
        {
            this.RedirectRoute = route;
            this.StatusCode = Util.Clamp(status, 300, 399);
        }
        internal override void Finish(ContextWrapper cw)
        {
            HttpListenerResponse res = cw.Context.Response;
            res.Redirect(RedirectRoute);
            res.StatusCode = this._StatusCode ?? 303;
            res.StatusDescription = null;
            base.Finish(cw);
        }
        public static new Response Default => new RedirectResponse("/");
    }

    public sealed class StringResponse : Response
    {
        public StringResponse(string response, HTTPStatus status = HTTPStatus.OK) : this(response, (Int32)status) { }
        public StringResponse(string response, int status)
        {
            this.data = Encoding.UTF8.GetBytes(response);
            this.StatusCode = status;
        }
        public static implicit operator StringResponse(string str) => new StringResponse(str);
        public static new Response Default => new StringResponse("");
    }

    public sealed class DataResponse : Response
    {
        public DataResponse(byte[] data, HTTPStatus status) : this(data, (Int32)status) { }
        public DataResponse(byte[] data) : this(data, (data == null) ? 501 : 200) { }
        public DataResponse(byte[] data, int status)
        {
            this.data = data;
            this.StatusCode = status;
        }
        public static implicit operator DataResponse(byte[] bytes) => new DataResponse(bytes);
        public static implicit operator DataResponse(FileIO.FileData file) => new DataResponse(file.Data);
        public static new Response Default => new DataResponse(new byte[] { });
    }

    public sealed class FileResponse : Response
    {
        internal string FileName;
        internal string BaseDirectory;
        public override byte[] Body
        {
            get
            {
                if(this.data == null) { this.data = FileIO.GetFile(FileName, BaseDirectory).Data; }
                return this.data;
            }
        }

        public override int StatusCode
        {
            get => (this.Body == null) ? 404 : this._StatusCode ?? 200;
            set => this._StatusCode = value;
        }

        public FileResponse(FileIO.FileData file) => this.data = file.Data;
        public FileResponse(string fileName) : this(fileName, Server.Config.StaticFilesBaseDirectory) { }
        public FileResponse(string fileName, string baseDirectory)
        {
            this.data = null;
            this.FileName = fileName;
            this.BaseDirectory = baseDirectory;
        }
        public static new Response Default => new FileResponse(null);
    }

    public sealed class EmptyResponse : Response
    {
        public EmptyResponse(HTTPStatus status) : this((Int32) status) { }
        public EmptyResponse(Int32 status)
        {
            this.StatusCode = status;
            this.data = null;
        }
        public static new Response Default => new EmptyResponse(HTTPStatus.BadRequest);
    }

    public sealed class NoResponse : Response
    {
        public NoResponse() { this.noResponse = true; }
        public static new NoResponse Default => new NoResponse();
    }

    public sealed class PassThrough : Response
    {
        internal PassThrough(bool skip) : this() => this.Skipped = skip;
        public PassThrough() : this(null, HTTPStatus.OK) { }
        public PassThrough(byte[] data, HTTPStatus status) : this(data, (Int32)status) { }
        public PassThrough(byte[] data, int status)
        {
            this.data = data;
            this.StatusCode = status;
        }
        public static new Response Default => new PassThrough();
        internal static Response Skip => new PassThrough(skip: true);
    }
}