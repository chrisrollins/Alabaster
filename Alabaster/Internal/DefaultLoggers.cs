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

        public static readonly Logger.Channel Console = new Logger.Channel(Names.Console, message => System.Console.WriteLine(message.Content));
        public static readonly Logger.Channel Info = new Logger.Channel(Names.Info, DefaultLoggers.Console);
        public static readonly Logger.Channel Error = new Logger.Channel(Names.Info, DefaultLoggers.Console);
        public static readonly Logger.Channel Default = new Logger.Channel(DefaultLoggers.Console);
        public static readonly Logger.Channel Debug =
        #if DEBUG
            new Logger.Channel(Names.Debug, DefaultLoggers.Console);
        #else
            new Logger.Channel(Names.Debug);
        #endif
    }
}
