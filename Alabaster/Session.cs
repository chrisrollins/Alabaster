using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;

namespace Alabaster
{
    public sealed class Session
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

        internal readonly Int64 id;
        internal readonly string key;
        internal readonly string category;

        private static Int64 count = 0;
        [ThreadStatic] private static Random rand;
        private static ConcurrentDictionary<Int64, Session> sessions = new ConcurrentDictionary<Int64, Session>(Environment.ProcessorCount, 100);

        public Session(string category)
        {
            this.category = category ?? throw new ArgumentNullException("Category must not be null.");
            if (rand == null) { SetupRNG(); }
            this.id = Interlocked.Increment(ref count);
            this.key = Guid.NewGuid().ToString() + rand.Next();
            sessions[this.id] = this;
        }

        internal static Session GetSession(Int64 id, string key)
        {
            sessions.TryGetValue(id, out Session val);
            return (val.key == key) ? val : null;
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
    }
}