using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alabaster
{
    internal sealed class SmartThreadPool
    {
        private readonly int concurrency;
        private readonly ConcurrentQueue<Action> workQueue = new ConcurrentQueue<Action>();
        private readonly ConcurrentBag<WorkerThread> availableThreads = new ConcurrentBag<WorkerThread>();
        private readonly ConcurrentDictionary<WorkerThread, bool> runningThreads = new ConcurrentDictionary<WorkerThread, bool>();
        private readonly object CheckConcurrencyLock = new object();
        private readonly object CheckSuspendedLock = new object();
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

        private WorkerThread GenerateThread()
        {
            WorkerThread wt = new WorkerThread();
            wt.ResetEvent = new AutoResetEvent(false);
            wt.InternalThread = new Thread(() =>
            {
                while (true)
                {
                    this.runningThreads.TryAdd(wt, true);
                    while (this.workQueue.TryDequeue(out Action work)) { work(); }
                    this.runningThreads.TryRemove(wt, out _);
                    this.availableThreads.Add(wt);
                    wt.ResetEvent.WaitOne();
                }
            });
            return wt;
        }

        public void QueueWork(Action work)
        {
            ConcurrentBag<WorkerThread> at = this.availableThreads;
            bool autoExpand = this.autoExpand;
            this.workQueue.Enqueue(work);
            lock (CheckConcurrencyLock)
            {
                if (runningThreads.Count < this.concurrency)
                {
                    StartWork();
                    return;
                }
            }

            lock (CheckSuspendedLock)
            {
                int running = 0;
                foreach (WorkerThread thread in runningThreads.Keys)
                {
                    if (thread.InternalThread.ThreadState == ThreadState.Running) { running++; }
                }
                if (running < this.concurrency) { StartWork(); }
            }            

            void StartWork()
            {
                WorkerThread thread;
                bool haveThread = at.TryTake(out thread);
                if(!haveThread && autoExpand)
                {
                    thread = GenerateThread();
                    haveThread = true;
                }
                if (haveThread)
                {
                    if (thread.InternalThread.ThreadState == ThreadState.Unstarted) { thread.InternalThread.Start(); }
                    else { thread.ResetEvent.Set(); }
                }
            }           
        }

        private struct WorkerThread
        {
            public Thread InternalThread;
            public AutoResetEvent ResetEvent;
        }
    }
}
