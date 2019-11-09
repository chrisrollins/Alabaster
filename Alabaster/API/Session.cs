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
        private readonly ConcurrentDictionary<string, object> sessionData = new ConcurrentDictionary<string, object>();
                
        private object this[string key]
        {
            get => (this.sessionData.TryGetValue(key, out object result) ? result : default);
            set => this.sessionData[key] = value;
        }

        public bool TryGetValue(string key, out string item) => TryGetValueImplementation(key, out item);
        public bool TryGetValue<T>(string key, out T item) where T : struct => TryGetValueImplementation(key, out item);
        private bool TryGetValueImplementation<T>(string key, out T item)
        {
            object o = this[key];
            if(o != null)
            {
                item = (T)o;
                return true;
            }
            item = default;
            return false;            
        }
        public void SetValue<T>(string key, T value) where T : struct => this[key] = value;
        public void SetValue(string key, string value) => this[key] = value;
        
        internal readonly string id;
        internal readonly string name;
        private long disposed = 0;
        private Intervals.IntervalCallback intervalCallback;
        private const int defaultDuration = 50;
        private static long sessionCount = long.MinValue + new Random().Next() + DateTime.Now.Millisecond;

        [ThreadStatic] private static Random rand;
        private static ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>(Environment.ProcessorCount, 100);
        
        internal Session(string name)
        {
            long count = Interlocked.Increment(ref sessionCount);
            this.name = name ?? throw new ArgumentNullException("Name must not be null.");
            this.id = Guid.NewGuid().ToString() + count;
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

        private void DisposedCheck()
        {
            if(Interlocked.Read(ref this.disposed) == 1) { throw new ObjectDisposedException("Session"); }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0) { sessions[this.id] = null; }
        }
    }

    public static partial class Server
    {
        public static void SessionInitializer(string name, Action<Session> callback) => SessionInitializer(name, (Session s, Request r) => callback(s));
        public static void SessionInitializer(string name, Action<Session, Request> callback)
        {
            RouteCallback initializer = new RouteCallback
            (
                priority: 1,
                cb: (req) =>
                {
                    if (req.GetSession(name) == null)
                    {
                        Session s = new Session(name);
                        callback(s, req);
                        return new PassThrough().AddSession(s);
                    }
                    return PassThrough.Skip;           
                }
            );
            Routing.AddHandler(initializer);
        }
    }
}