using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Alabaster
{    
    public static class DefaultLoggers
    {
        private static class Names
        {
            private const string BasePrefix = "Alabaster-Logger-";
            public const string Blank = "";
            public const string Debug = BasePrefix + "Debug";
            public const string Info = BasePrefix + "Info";
            public const string Error = BasePrefix + "Error";
        }
        
        public static readonly Logger.Channel Console = (name: Names.Blank, handler: message => System.Console.WriteLine(message.Content));
        public static readonly Logger.Channel WithThreadID = (name: Names.Blank, handler: (Logger.Message message) => (Logger.Message)$"[Thread {message.OriginThread.ManagedThreadId}] {message.Content}", receiver: DefaultLoggers.Console);
        public static readonly Logger.Channel WithTimestamp = (name: Names.Blank, handler: (Logger.Message message) => (Logger.Message)$"[{DateTime.Now.ToString()}] {message.Content}", receiver: DefaultLoggers.Console);
        public static readonly Logger.Channel Default = (name: Names.Blank, receiver: Logger.Channel.Chain(WithThreadID, WithTimestamp));
        public static readonly Logger.Channel Info = (name: Names.Info, receiver: DefaultLoggers.Default);
        public static readonly Logger.Channel Error = (name: Names.Error, receiver: DefaultLoggers.Default);
        public static readonly Logger.Channel Debug =
        #if DEBUG
            (name: Names.Debug, receiver: DefaultLoggers.Default);
        #else
            new Logger.Channel(name: Names.Debug);
        #endif
    }
}
