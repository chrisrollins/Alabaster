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
        private static ConcurrentQueue<IntervalCallback> hourly = new ConcurrentQueue<IntervalCallback>();
        private static ConcurrentQueue<IntervalCallback> daily = new ConcurrentQueue<IntervalCallback>();
        private static Thread workThread = new Thread(HandleActions);
        private static object syncLock = new object();
        private static int delay = 0;
        private const int hourMS = 3600000;
        private static ConcurrentDictionary<int, bool> IntervalThreadIDs = new ConcurrentDictionary<int, bool>();

        static Intervals()
        {
            delay = hourMS - (DateTime.Now.Millisecond + DateTime.Now.Second * 1000 + DateTime.Now.Minute * 60000);
            workThread.Start();
        }
        
        internal static void DailyJob(IntervalCallback callback) => AddJob(callback, daily);
        internal static void HourlyJob(IntervalCallback callback) => AddJob(callback, hourly);
        private static void AddJob(IntervalCallback callback, ConcurrentQueue<IntervalCallback> queue)
        {
            IntervalThreadIDs.TryGetValue(Thread.CurrentThread.ManagedThreadId, out bool isIntervalThread);
            if(isIntervalThread) { throw new InvalidOperationException("Can't schedule job from a job callback."); }
            lock (syncLock) { queue.Enqueue(callback); }
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
                Thread.Sleep(delay);
                delay = hourMS;
                lock (syncLock)
                {
                    processQueue(hourly);
                    if (DateTime.Now.Hour == 0) { processQueue(daily); }
                }
            }

            void processQueue(ConcurrentQueue<IntervalCallback> queue)
            {
                Queue<IntervalCallback> tempQueue = new Queue<IntervalCallback>();
                while (queue.Count > 0)
                {
                    queue.TryDequeue(out IntervalCallback callback);
                    callback.SubtractTimes(1);
                    Thread t = new Thread(() =>
                    {
                        callback.Work();
                        IntervalThreadIDs.TryRemove(Thread.CurrentThread.ManagedThreadId, out bool _);
                    });
                    IntervalThreadIDs.TryAdd(t.ManagedThreadId, true);
                    t.Start();
                    if (callback.RemainingTimes > 0) { tempQueue.Enqueue(callback); }
                }
                while (tempQueue.Count > 0)
                {
                    queue.Enqueue(tempQueue.Dequeue());
                }
            }
        }
    }
}
