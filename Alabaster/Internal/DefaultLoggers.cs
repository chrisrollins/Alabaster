using System;
using System.Collections.Generic;
using System.Text;

namespace Alabaster
{    
    static class DefaultLoggers
    {
        private static class Names
        {
            private const string BasePrefix = "Alabaster-Logger-";
            public const string Console = "";
            public const string Debug = BasePrefix + "Debug";
            public const string Info = BasePrefix + "Info";
            public const string Error = BasePrefix + "Error";
        }

        public static readonly Logger.Channel Console = (name: Names.Console, handler: message => System.Console.WriteLine(message.Content));
        public static readonly Logger.Channel Default = (name: Names.Console, receiver: DefaultLoggers.Console);
        public static readonly Logger.Channel Info = (name: Names.Info, receiver: DefaultLoggers.Default);
        public static readonly Logger.Channel Error = (name: Names.Error, receiver: DefaultLoggers.Default);
        public static readonly Logger.Channel Debug =
        #if DEBUG
            (name: Names.Debug, receiver: DefaultLoggers.Console);
        #else
            new Logger.Channel(name: Names.Debug);
        #endif
    }
}
