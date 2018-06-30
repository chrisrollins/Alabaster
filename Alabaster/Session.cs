using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;

namespace Alabaster
{
    public sealed class Session : IDisposable
    {
        private ValueType data;
        private object dataSync = new object();

        public ValueType Data
        {
            get { lock (dataSync) { return data; } }
            set
            {
                lock (dataSync)
                {
                    data = value;
                }
            }
        }

        internal readonly string id;
        internal readonly string category;
        private long disposed = 0;
        private Intervals.IntervalCallback intervalCallback;
        private const int defaultDuration = 50;

        [ThreadStatic] private static Random rand;
        private static ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>(Environment.ProcessorCount, 100);
        internal const string CookieID = "AlabasterSessionID";
        
        public Session(string category, ValueType data)
        {
            this.category = category ?? throw new ArgumentNullException("Category must not be null.");
            this.data = data ?? throw new ArgumentNullException("Data cannot be null.");
            this.id = GenerateSessionID();
            this.intervalCallback = new Intervals.IntervalCallback();
            this.intervalCallback.SetTimes(defaultDuration);
            this.intervalCallback.Work = () =>
            {
                if (this.intervalCallback.RemainingTimes == 0) { this?.Dispose(); }
            };
            Thread.MemoryBarrier();
            Intervals.DailyJob(this.intervalCallback);
            sessions[this.id] = this;
        }

        internal static Session GetSession(string id)
        {
            sessions.TryGetValue(id, out Session session);
            session?.intervalCallback.SetTimes(defaultDuration);
            return session;
        }
                
        private static string GenerateSessionID()
        {
            if (rand == null) { SetupRNG(); }
            string id;
            int count = 0;
            do
            {
                id = Guid.NewGuid().ToString() + rand.Next();
                if(count++ > 1000) { throw new Exception("Could not generate unique session GUID."); }
            } while (sessions.ContainsKey(id));
            return id;
        }

        private static void SetupRNG()
        {
            rand = new Random();
            int dummyRolls = (Thread.CurrentThread.ManagedThreadId + (Server.Config.Port % 11)) % 100;
            for(int i = 0; i < dummyRolls; i++)
            {
                int n = rand.Next();
            }
        }

        private void DisposedCheck()
        {
            if(Interlocked.Read(ref this.disposed) == 1) { throw new ObjectDisposedException("Session"); }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0) { sessions[this.id] = null; }
        }
    }
}