using System;

namespace Alabaster
{
    public struct ServerOptions
    {
        public bool EnableCustomHTTPMethods;
        private string serverID;

        public string ServerID
        {
            get
            {
                if (this.serverID == null) { this.serverID = new Guid().ToString(); }
                return this.serverID;
            }
            set => this.serverID = value;
        }

        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                Util.InitExceptions();
                _port = Util.Clamp(value, 1, UInt16.MaxValue);
            }
        }
    }
}
