﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Alabaster
{
    internal static class Util
    {
        internal static string GetFileExtension(string filename)
        {
            for (int i = filename.Length - 1; i > 0; i--)
            {
                if (filename[i] == '.') { return filename.Substring(i + 1); }
            }
            return null;
        }

        internal static void ProgressVisualizer(string startLabel, string endLabel, params Action[] functions)
        {
            Console.WriteLine(startLabel);
            int barLength = 100 + (functions.Length - (100 % functions.Length));
            int chunkSize = barLength / functions.Length;
            string progressChunk = new string(':', chunkSize);
            Console.Write('[');
            Console.CursorLeft = barLength + 1;
            Console.Write(']');
            for (int i = 0; i < functions.Length; i++)
            {
                functions[i]();
                Console.CursorLeft = chunkSize * i + 1;
                Console.Write(progressChunk);
            }
            Console.CursorLeft = 0;
            Console.WriteLine(string.Join(null, '\n', endLabel, new string(' ', barLength)));
        }

        internal static string ReplaceMultiple(string str, string toReplace, char replacement)
        {
            char[] newChars = str.ToCharArray();
            for (int i = 0; i < newChars.Length; i++)
            {
                if (toReplace.Contains(newChars[i])) { newChars[i] = replacement; }
            }
            return new string(newChars);
        }

        internal static string RemoveMultiple(string str, string toRemove)
        {
            List<char> newChars = new List<char>(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                if (!toRemove.Contains(newChars[i])) { newChars.Add(toRemove[i]); }
            }
            return string.Join(null, newChars);
        }
        
        internal static T Clamp<T>(T value, T min, T max) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T> => (value.CompareTo(max) > 0) ? max : (value.CompareTo(min) < 0) ? min : value;

        internal class CustomEqualityComparer<T> : IEqualityComparer<T>
        {
            private Func<T, T, bool> _Equals;
            private Func<T, int> _GetHashCode;
            public CustomEqualityComparer(Func<T, T, bool> Equals, Func<T, int> GetHashCode) => (this._Equals, this._GetHashCode) = (Equals, GetHashCode);
            public bool Equals(T x, T y) => this._Equals(x, y);
            public int GetHashCode(T obj) => this._GetHashCode(obj);
        }

        internal static void ForEach<T>(this IEnumerable<T> items, Action<T, int> callback)
        {
            T[] evaluatedCollection = items.ToArray();            
            for (int i = 0; i < evaluatedCollection.Length; i++)
            {
                callback(evaluatedCollection.ElementAt(i), i);
            }
        }
        internal static void ForEach<T>(this IEnumerable<T> items, Action<T> callback) => items.ForEach((item, i) => callback(item));
    }
}
