using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Alabaster
{
    //this class manages the server's main thread. this is not the same thread as the application's main thread.
    //most calls to the API are run as jobs on this thread. the purpose of this is to gain thread safety on setup operations.
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
