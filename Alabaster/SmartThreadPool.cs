using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alabaster
{
    internal class SmartThreadPool
    {
        private readonly int concurrency;
        private ConcurrentQueue<Action> workQueue = new ConcurrentQueue<Action>();
        private ConcurrentBag<Thread> availableThreads = new ConcurrentBag<Thread>();
        private ConcurrentDictionary<Thread, bool> runningThreads = new ConcurrentDictionary<Thread, bool>();
        private object CheckConcurrencyLock = new object();
        private object CheckSuspendedLock = new object();
        private readonly bool autoExpand;

        public SmartThreadPool(int concurrency, int initialThreadCount, bool autoExpand)
        {
            this.concurrency = concurrency;
            this.autoExpand = autoExpand;
            for (int i = 0; i < initialThreadCount; i++)
            {
                this.availableThreads.Add(this.GenerateThread());
            }
        }

        private Thread GenerateThread()
        {
            return new Thread((object callback) =>
            {
                (callback as Action)();
                while (this.workQueue.TryDequeue(out Action work)) { work(); }
                this.runningThreads.TryRemove(Thread.CurrentThread, out bool junk);
                this.availableThreads.Add(Thread.CurrentThread);
            });
        }

        public void QueueWork(Action work)
        {
            if(!TryStart()) { this.workQueue.Enqueue(work); }

            bool TryStart()
            {
                lock (CheckConcurrencyLock)
                {
                    if (runningThreads.Count < this.concurrency) { return Start(); }
                }

                lock (CheckSuspendedLock)
                {
                    int running = 0;
                    foreach (Thread thread in runningThreads.Keys)
                    {
                        if (thread.ThreadState == ThreadState.Running) { running++; }
                    }
                    if (running < this.concurrency) { return Start(); }
                }

                return false;
            }

            bool Start()
            {
                Thread thread = availableThreads.TryTake(out thread) ? thread : (this.autoExpand) ? GenerateThread() : null;
                if (thread != null)
                {
                    thread.Start(work);
                    runningThreads.TryAdd(thread, true);
                    return true;
                }
                return false;
            }           
        }
    }
}
