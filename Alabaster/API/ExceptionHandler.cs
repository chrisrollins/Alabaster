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
            internal Response Resolve(ExceptionInfo exceptionInfo) => (exceptionInfo.Exception.GetType() == this.ExceptionType) ? Handler(exceptionInfo) : new PassThrough();
        }

        public static void AddExceptionHandler<T>(ExceptionHandler callback) where T : Exception => ServerThreadManager.Run(() => AddExceptionHandlerInternal<T>(callback));
        private static void AddExceptionHandlerInternal<T>(ExceptionHandler callback) where T : Exception
        {
            Util.InitExceptions();
            exceptionHandlerAddList.Add((callback, typeof(T)));            
        }

        private static Response ResolveException(Exception e, ContextWrapper cw) => ResolveException((e, new Request(cw)));
        private static Response ResolveException(ExceptionInfo exceptionInfo)
        {
            Response result = null;
            try
            {
                foreach (ExceptionHandlerResolver ehr in finalizedExceptionHandlers)
                {
                    result = ehr.Resolve(exceptionInfo);
                    if (!(result is PassThrough)) { break; }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception occurred in exception handler:");
                Console.WriteLine(e);
                result = 500;
            }
            return (result is PassThrough) ? (result._StatusCode ?? 500) : result;
        }

        private static void FinalizeExceptionHandlers()
        {
            AddExceptionHandlerInternal<Exception>((ExceptionInfo exceptionInfo) =>
            {
                Console.WriteLine("Exception while handling request:");
                Console.WriteLine(exceptionInfo.Exception);
                Console.WriteLine("Request URL path: \"" + exceptionInfo.Request.Route + "\"");
                Console.WriteLine("Request HTTP method: \"" + exceptionInfo.Request.HttpMethod + "\"");
                return 500;
            });
            finalizedExceptionHandlers = exceptionHandlerAddList.ToArray();
            exceptionHandlerAddList = null;
        }
    }
}
