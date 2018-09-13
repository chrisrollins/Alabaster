using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Alabaster
{
    using ResponseSentCallback_A = Action<Request, Response>;
    using ResponseSentCallback_B = Action<Response>;

    public partial class Server
    {
        internal static List<ResponseSentCallback_A> ResponseSentCallbacks = new List<ResponseSentCallback_A>(1);

        public static void OnResponseSent(ResponseSentCallback_B callback) => OnResponseSent((Request req, Response res) => callback(res));
        public static void OnResponseSent(ResponseSentCallback_A callback)
        {
            ServerThreadManager.Run(() =>
            {
                Util.InitExceptions();
                OnResponseSentInternal(callback);
            });
        }

        private static void OnResponseSentInternal(ResponseSentCallback_A callback) => ResponseSentCallbacks.Add(callback);        
    }

    public abstract partial class Response
    {
        private void AdditionalFinishTasks(Request req, Response res)
        {
            if(Server.ResponseSentCallbacks.Count == 0) { return; }
            foreach(ResponseSentCallback_A cb in Server.ResponseSentCallbacks)
            {
                cb(req, res);
            }            
        }
    }
}
