using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using VariableKey = System.ValueTuple<string, System.Type, System.Object>;

namespace Alabaster
{
    public static class GlobalData
    {
        private static readonly object DefaultObject = new object();
        private static readonly ConcurrentDictionary<VariableKey, object> dict = new ConcurrentDictionary<VariableKey, object>(Environment.ProcessorCount, 100);

        public static void StoreVariable<T>(string name, T value) where T : struct => StoreInternal((name, typeof(T), DefaultObject), value);
        public static string RetrieveVariable<T>(string name) where T : struct => RetrieveInternal((name, typeof(T), DefaultObject));
        public static void StoreVariable(string name, string value) => StoreInternal((name, typeof(string), DefaultObject), value);
        public static string RetrieveVariable(string name) => RetrieveInternal((name, typeof(string), DefaultObject));

        internal static void StoreInternal<T>(VariableKey key, T value) where T : struct => dict[key] = value;
        internal static void StoreInternal(VariableKey key, string value) => dict[key] = value;
        internal static T RetrieveInternal<T>(VariableKey key) where T : struct => (T)(dict.TryGetValue(key, out object result) ? result : default);
        internal static string RetrieveInternal(VariableKey key) => (string)(dict.TryGetValue(key, out object result) ? result : default);
    }
}