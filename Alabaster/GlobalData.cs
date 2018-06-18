using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using VariableKey = System.ValueTuple<string, System.Type>;

namespace Alabaster
{
    public static class GlobalData
    {
        private static ConcurrentDictionary<VariableKey, object> dict = new ConcurrentDictionary<VariableKey, object>(Environment.ProcessorCount, 100);

        public static void StoreVariable<T>(string name, T value) where T : struct => dict[(name, typeof(T))] = value;

        public static T RetrieveVariable<T>(string name) where T : struct
        {
            dict.TryGetValue((name, typeof(T)), out object value);
            return (T)(value ?? new T());
        }

        public static void StoreVariable(string name, string value) => dict[(name, typeof(string))] = value;

        public static string RetrieveVariable(string name)
        {
            dict.TryGetValue((name, typeof(string)), out object value);
            return (string)(value ?? "");
        }
    }
}