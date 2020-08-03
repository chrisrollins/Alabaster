using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Alabaster
{
    internal static class InternalQueueManager
    {
        internal static ActionQueue SetupQueue = new ActionQueue();

        internal class ActionQueue
        {
            private Thread Thread;
            private BlockingCollection<Action> Queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
            internal void Run(Action callback) => this.Queue.Add(callback);

            internal struct Configuration
            {
                public ThreadPriority Priority;

                public static Configuration Default = new Configuration
                {
                    Priority = ThreadPriority.Normal,
                };
            }

            internal ActionQueue() : this(Configuration.Default) { }
            internal ActionQueue(Configuration configuration)
            {
                this.Thread = new Thread(() =>
                {
                    while (true)
                    {
                        this.Queue.TryTake(out Action action);
                        action?.Invoke();
                    }
                });
                this.Thread.Priority = configuration.Priority;
                this.Thread.Start();
            }
        }
    }
}
