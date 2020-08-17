using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public readonly ref struct ExceptionInfo
    {
        public readonly Exception Exception;
        public readonly Request Request;
        internal ExceptionInfo(Exception e, Request r)
        {
            this.Exception = e;
            this.Request = r;
        }
        public static implicit operator ExceptionInfo((Exception e, Request r) args) => new ExceptionInfo(args.e, args.r);
    }

    public delegate Response ExceptionHandler(ExceptionInfo exceptionInfo);

    public static partial class Server
    {
        private static List<ExceptionHandlerResolver> exceptionHandlerAddList = new List<ExceptionHandlerResolver>(100);
        private static ExceptionHandlerResolver[] finalizedExceptionHandlers;

        private readonly struct ExceptionHandlerResolver
        {
            internal readonly ExceptionHandler Handler;
            internal readonly Type ExceptionType;
            internal ExceptionHandlerResolver(ExceptionHandler h, Type t)
            {
                this.Handler = h;
                this.ExceptionType = t;
            }
            public static implicit operator ExceptionHandlerResolver((ExceptionHandler h, Type t) args) => new ExceptionHandlerResolver(args.h, args.t);
            internal Response Resolve(ExceptionInfo exceptionInfo) => (MatchException(exceptionInfo.Exception)) ? Handler(exceptionInfo) : PassThrough.Default;

            private bool MatchException(Exception e)
            {
                if(this.ExceptionType == typeof(Exception)) { return true; }
                Type t = e.GetType();
                while(this.ExceptionType != t)
                {
                    t = t.BaseType;
                    if (t == typeof(object)) { return false; }
                }
                return true;
            }
        }

        /// <summary>Registers a handler for exceptions encountered while processing HTTP requests.</summary>
        /// <exception cref="InvalidOperationException">Thrown if this is called after server was started.</exception>
        public static void AddExceptionHandler<T>(ExceptionHandler callback) where T : Exception => InternalQueueManager.SetupQueue.Run(() => AddExceptionHandlerInternal<T>(callback));
        
        private static void AddExceptionHandlerInternal<T>(ExceptionHandler callback) where T : Exception => exceptionHandlerAddList.Add((callback, typeof(T)));            
        
        private static Response ResolveException(Exception e, ContextWrapper cw) => ResolveException((e, new Request(cw)));
        private static Response ResolveException(ExceptionInfo exceptionInfo)
        {
            Response result = null;
            try
            {
                foreach (ExceptionHandlerResolver ehr in finalizedExceptionHandlers)
                {
                    result = ehr.Resolve(exceptionInfo);
                    if (!result.Skipped) { result.Merge(exceptionInfo.Request.cw); }
                    if (!(result is PassThrough)) { break; }
                }
            }
            catch (Exception e)
            {
                DefaultLoggers.Error
                .Log("Exception occurred in exception handler:")
                .Log(e);
                result = HTTPStatus.InternalServerError;
            }
            return (result is PassThrough) ? (result._StatusCode ?? (Int32)HTTPStatus.InternalServerError) : result;
        }

        private static void FinalizeExceptionHandlers()
        {
            AddExceptionHandlerInternal<Exception>((ExceptionInfo exceptionInfo) =>
            {
                DefaultLoggers.Error
                .Log("Exception while handling request:")
                .Log(exceptionInfo.Exception)
                .Log("Request URL path: \"" + exceptionInfo.Request.Route + "\"")
                .Log("Request HTTP method: \"" + exceptionInfo.Request.HttpMethod + "\"");
                return HTTPStatus.InternalServerError;
            });
            finalizedExceptionHandlers = exceptionHandlerAddList.ToArray();
            exceptionHandlerAddList = null;
        }
    }
}
