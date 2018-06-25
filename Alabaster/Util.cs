using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        internal static void InitExceptions(Action callback = null)
        {
            ThreadExceptions();
            if (Server.initialized) { throw new InvalidOperationException("Cannot use initialization operations after server has started."); }
            callback?.Invoke();
        }

        internal static void ThreadExceptions(Action callback = null)
        {
            if (Server.baseThread != Thread.CurrentThread) { throw new InvalidOperationException("Server setup must be done on one thread."); }
            callback?.Invoke();
        }

        internal static readonly string[] standardHTTPMethods = { "GET", "POST", "PATCH", "PUT", "DELETE", "HEAD", "CONNECT", "OPTIONS", "TRACE" };
        internal static void httpMethodExceptions(string method)
        {
            if (!Server.Config.EnableCustomHTTPMethods && !Util.standardHTTPMethods.Contains(method.ToUpper())) { throw new ArgumentException("Non-standard HTTP method: " + method + " Enable non-standard HTTP methods to use a custom method by setting Server.EnableCustomHTTPMethods to true."); }
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
