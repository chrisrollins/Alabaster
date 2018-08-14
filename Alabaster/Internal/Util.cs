using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            Console.WriteLine(String.Join(null, '\n', endLabel, new string(' ', barLength)));
        }

        internal static void ThrowIf(bool condition, Exception e)
        {
            if (!condition) { throw e; }
        }
        
        internal static void InitExceptions(Action callback = null)
        {
            if (Server.initialized) { throw new InvalidOperationException("Cannot use initialization operations after server has started."); }
            callback?.Invoke();
        }
        
        internal static T Clamp<T>(T value, T min, T max) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T> => (value.CompareTo(max) > 0) ? max : (value.CompareTo(min) < 0) ? min : value;

        internal static Task TaskWithExceptionHandler(Action callback, Action<Exception> handler = null) => new Task(() => ExceptionHandlerCallback(callback, handler));
        internal static Task RunTaskWithExceptionHandler(Action callback, Action<Exception> handler = null) => Task.Run(() => ExceptionHandlerCallback(callback, handler));
        private static void ExceptionHandlerCallback(Action callback, Action<Exception> handler)
        {
            try { callback(); }
            catch (Exception e)
            {
                if (handler == null)
                {
                    Console.WriteLine("Exception on a background thread:");
                    Console.WriteLine(e);
                }
                else { handler(e); }
            }
        }
    }
}
