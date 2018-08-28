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
        private static BlockingCollection<Action> ActionQueue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
        private static Thread MainThread = new Thread(() =>
        {
            while (true)
            {
                ActionQueue.TryTake(out Action action);
                action?.Invoke();                
            }
        });
        static ServerThreadManager() => MainThread.Start();
        public static void Run(Action callback) => ActionQueue.Add(callback);        
    }
}
