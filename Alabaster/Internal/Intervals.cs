using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Alabaster
{
    //this class can run jobs hourly or daily while the server is running. it's used to implement Session lifetimes.
    internal static class Intervals
    {
        private static ConcurrentBag<IntervalCallback> Hourly = new ConcurrentBag<IntervalCallback>();
        private static ConcurrentBag<IntervalCallback> Daily = new ConcurrentBag<IntervalCallback>();
        private static InternalQueueManager.ActionQueue WorkQueue = new InternalQueueManager.ActionQueue();
        private static object SyncLock = new object();
        private static int Delay = 0;
        private const int HourMS = 3600000;
        private static ConcurrentDictionary<int, bool> IntervalThreadIDs = new ConcurrentDictionary<int, bool>();

        static Intervals()
        {
            Delay = HourMS - (DateTime.Now.Millisecond + DateTime.Now.Second * 1000 + DateTime.Now.Minute * 60000);
            WorkQueue.Run(HandleActions);
        }
        
        internal static void DailyJob(IntervalCallback callback) => AddJob(callback, Daily);
        internal static void HourlyJob(IntervalCallback callback) => AddJob(callback, Hourly);
        private static void AddJob(IntervalCallback callback, ConcurrentBag<IntervalCallback> queue)
        {
            IntervalThreadIDs.TryGetValue(Thread.CurrentThread.ManagedThreadId, out bool isIntervalThread);
            if(isIntervalThread) { throw new InvalidOperationException("Can't schedule job from a job callback."); }
            lock (SyncLock) { queue.Add(callback); }
        }
        
        internal struct IntervalCallback
        {
            private volatile int _remaining;

            public Action Work;
            public int RemainingTimes { get => this._remaining; }

            public void AddTimes(int add) => Interlocked.Add(ref this._remaining, add);
            public void SubtractTimes(int subtract) => Interlocked.Add(ref this._remaining, -subtract);
            public void SetTimes(int newVal) => Interlocked.Exchange(ref this._remaining, newVal);
        }

        private static void HandleActions()
        {
            while (true)
            {
                Thread.Sleep(Delay);
                Delay = HourMS;
                lock (SyncLock)
                {
                    processQueue(Hourly);
                    if (DateTime.Now.Hour == 0) { processQueue(Daily); }
                }
            }

            void processQueue(ConcurrentBag<IntervalCallback> queue)
            {
                var reQueue = new List<IntervalCallback>();
                while(queue.TryTake(out var callback))
                {
                    callback.SubtractTimes(1);
                    var actionQueue = new InternalQueueManager.ActionQueue();
                    if (IntervalThreadIDs.TryAdd(actionQueue.Thread.ManagedThreadId, true))
                    {
                        actionQueue.Run(() =>
                        {
                            InternalExceptionHandler.Try(callback.Work);
                            IntervalThreadIDs.TryRemove(Thread.CurrentThread.ManagedThreadId, out bool _);
                        });
                    }
                    else
                    {
                        actionQueue.Throw(InternalExceptionCode.FailedToRegisterIntervalThreadID);
                    }              
                    if (callback.RemainingTimes > 0) { reQueue.Add(callback); }
                }
                reQueue.ForEach(callback => queue.Add(callback));
            }
        }
    }
}
