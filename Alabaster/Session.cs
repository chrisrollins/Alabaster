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

        internal readonly Int32 key;
        internal readonly Int64 id;

        private static Int64 count = 0;
        [ThreadStatic] private static Random rand;
        private static ConcurrentDictionary<Int64, Session> sessions = new ConcurrentDictionary<Int64, Session>(Environment.ProcessorCount, 100);
        
        public Session()
        {
            if(rand == null) { SetupRNG(); }
            this.key = rand.Next();
            this.id = Interlocked.Increment(ref count);
            sessions[this.id] = this;
        }

        public static Session GetSession(Int64 id, Int32 key)
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