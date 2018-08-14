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
        private ConcurrentBag<WorkerThread> availableThreads = new ConcurrentBag<WorkerThread>();
        private ConcurrentDictionary<WorkerThread, bool> runningThreads = new ConcurrentDictionary<WorkerThread, bool>();
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

        private WorkerThread GenerateThread()
        {
            WorkerThread wt = new WorkerThread();
            wt.ResetEvent = new AutoResetEvent(false);
            wt.InternalThread = new Thread(() =>
            {
                while (true)
                {
                    runningThreads.TryAdd(wt, true);
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
            this.workQueue.Enqueue(work);

            lock (CheckConcurrencyLock)
            {
                if (runningThreads.Count < this.concurrency)
                {
                    Start();
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
                if (running < this.concurrency) { Start(); }
            }            

            void Start()
            {
                WorkerThread thread;
                bool haveThread = availableThreads.TryTake(out thread);
                if(!haveThread && this.autoExpand)
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
