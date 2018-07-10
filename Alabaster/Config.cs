using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alabaster
{
    public static partial class Server
    {
        private static int _port = 0;
        private static int port
        {
            get => _port;
            set
            {
                Util.InitExceptions();
                _port = Util.Clamp(value, 1, UInt16.MaxValue);
            }
        }

        public static class Config
        {
            public static int Port { get => port; set => port = value; }
            public static bool EnableCustomHTTPMethods = false;
            public static string ServerID;
        }
    }
}
