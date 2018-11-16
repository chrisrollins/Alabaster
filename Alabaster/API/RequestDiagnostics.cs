using System;
using System.Collections.Generic;

namespace Alabaster
{
    using RouteCallback_A = Func<Request, Response>;
    using RouteCallback_B = Action<Request>;
    using RouteCallback_C = Func<Response>;
    using RouteCallback_D = Action;

    public sealed class RequestDiagnostics
    {
        private readonly List<HandlerResult> handlers = new List<HandlerResult>(10);
        private readonly List<string> currentHandlerMessages = new List<string>(10);
        private RouteCallback_A currentHandler;        
        public HandlerResult[] Results => handlers.ToArray();

        public void PostMessage(string message) => currentHandlerMessages.Add(message);        

        internal void PushHandler(RouteCallback_A handler)
        {
            currentHandler = handler;
        }

        internal void PushResult(Response result)
        {
            handlers.Add(new HandlerResult(currentHandler, result, currentHandlerMessages.ToArray()));
            currentHandlerMessages.Clear();
            currentHandler = null;
        }
        
        public readonly struct HandlerResult
        {
            public readonly RouteCallback_A Handler;
            public readonly Response Result;
            public readonly string[] Messages;
            internal HandlerResult(RouteCallback_A handler, Response result, string[] messages)
            {
                this.Handler = handler;
                this.Result = result;
                this.Messages = messages;
            }
        }
    }
}
