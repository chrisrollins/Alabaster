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
            internal readonly Thread Thread;
            private BlockingCollection<Action> Queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());

            internal void Run(Action callback) => this.Queue.Add(callback);

            internal void Throw(InternalExceptionCode errorCode) => this.Run(() => throw new InternalException(errorCode));

            internal struct Configuration
            {
                public ThreadPriority Priority;
                public Action<Exception> ExceptionHandler;

                public static Configuration Default = new Configuration
                {
                    Priority = ThreadPriority.Normal,
                    ExceptionHandler = (exception) => InternalExceptionHandler.Handle(exception)    
                };
            }

            internal ActionQueue() : this(Configuration.Default) { }
            internal ActionQueue(Configuration configuration)
            {
                this.Thread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Action action = null;
                            InternalExceptionHandler.Rethrow(InternalExceptionCode.ActionQueueTryTake, () => this.Queue.TryTake(out action));
                            action?.Invoke();
                        }
                        catch (Exception exception)
                        {
                            configuration.ExceptionHandler?.Invoke(exception);
                        }
                    }
                    
                });
                this.Thread.Priority = configuration.Priority;
                this.Thread.Start();
            }
        }
    }
}
