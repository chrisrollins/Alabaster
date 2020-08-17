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
        internal static readonly ActionQueue SetupQueue = new ActionQueue(new ActionQueue.Configuration {
            Priority = ThreadPriority.Normal,
            ExceptionHandler = ActionQueue.Configuration.Default.ExceptionHandler,
            OnRejected = () => InternalExceptionHandler.Handle(new InvalidOperationException("Cannot use initialization operations after server has started.")),
        });
        private static readonly Action AbortCommand = () => { };

        internal class ActionQueue
        {
            internal Thread Thread { get; private set; }
            private BlockingCollection<Action> Queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
            private Action OnRejected;
            private int Stopped = 0;

            internal void Run(Action callback)
            {
                if (Interlocked.Equals(this.Stopped, 1))
                {
                    if (callback == AbortCommand) { return; }
                    this.OnRejected?.Invoke();
                    return;
                }                                
                this.Queue.Add(callback);
            }

            internal void Stop()
            {
                if (Interlocked.CompareExchange(ref this.Stopped, 1, 0) == 1) { return; }
                this.Run(AbortCommand);
            }

            internal void Throw(InternalExceptionCode errorCode) => this.Run(() => throw new InternalException(errorCode));

            internal struct Configuration
            {
                internal ThreadPriority Priority;
                internal Action<Exception> ExceptionHandler;
                internal Action OnRejected;

                internal static Configuration Default = new Configuration
                {
                    Priority = ThreadPriority.Normal,
                    ExceptionHandler = (exception) => InternalExceptionHandler.Handle(exception),
                    OnRejected = () => InternalExceptionHandler.Handle(new InternalException(InternalExceptionCode.ActionQueueRejectedOperation)),
                };
            }

            internal ActionQueue() : this(Configuration.Default) { }
            internal ActionQueue(Configuration configuration) => this.Initialize(configuration);            

            private void Initialize(Configuration configuration)
            {
                if (Interlocked.Equals(this.Stopped, 1)) { return; }
                this.OnRejected = configuration.OnRejected;
                this.Thread = new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            Action action = null;
                            InternalExceptionHandler.Rethrow(InternalExceptionCode.ActionQueueTryTake, () => this.Queue.TryTake(out action));
                            if (action == AbortCommand) { return; }
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
