using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Alabaster
{
    internal static class ServerThreadManager
    {
        private static ConcurrentQueue<Action> ActionQueue = new ConcurrentQueue<Action>();
        private static AutoResetEvent NewActionEvent = new AutoResetEvent(false);
        private static Thread MainThread = new Thread(() =>
        {
            while (true)
            {
                NewActionEvent.WaitOne();
                while (ActionQueue.TryDequeue(out Action action))
                {
                    action?.Invoke();
                }
            }
        });

        public static void Run(Action callback)
        {
            ActionQueue.Enqueue(callback);
            NewActionEvent.Set();            
        }
    }
}
