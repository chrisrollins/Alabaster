using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Alabaster
{
    internal class LockableDictionary<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, ValueContainer> dict;
        private object SetLockLock = new object();

        internal LockableDictionary() => this.dict = new ConcurrentDictionary<TKey, ValueContainer>();
        internal LockableDictionary(int capacity) => this.dict = new ConcurrentDictionary<TKey, ValueContainer>(Environment.ProcessorCount, capacity);

        public TValue this[TKey key]
        {
            get
            {
                ValueContainer container = this.dict[key];
                return (container.ValueIsValid) ? this.dict[key].Value : throw new KeyNotFoundException("The given key was not present in the dictionary.");
            }
            set
            {
                dict.TryGetValue(key, out ValueContainer container);
                if (!container.Locked)
                {
                    this.dict[key] = new ValueContainer { Value = value, Locked = false, ValueIsValid = true };
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool result = this.dict.TryGetValue(key, out ValueContainer container) && container.ValueIsValid;            
            value = (result) ? container.Value : default;
            return result;
        }

        public void Lock(TKey key) => this.SetLock(key, true);
        public void Unlock(TKey key) => this.SetLock(key, false);

        private void SetLock(TKey key, bool state)
        {
            lock (SetLockLock)
            {
                if (!dict.TryGetValue(key, out ValueContainer container)) { container = new ValueContainer(); }
                container.Locked = state;
                dict[key] = container;
            }
        }

        public int Count => this.dict.Count;
                
        private struct ValueContainer
        {
            public TValue Value;
            public bool Locked;
            public bool ValueIsValid;
        }
    }
}
