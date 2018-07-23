using System;

namespace Alabaster
{
    public struct ServerOptions
    {
        public bool EnableCustomHTTPMethods;
        private string _serverID;

        public string ServerID
        {
            get
            {
                if (this._serverID == null) { this._serverID = new Guid().ToString(); }
                return this._serverID;
            }
            set => this._serverID = value;
        }

        private int _port;
        public int Port
        {
            get => _port;
            set => _port = Util.Clamp(value, 1, UInt16.MaxValue);            
        }

        private Int64 _maximumCacheFileSize;
        public Int64 MaximumCacheFileSize
        {
            get
            {
                if(_maximumCacheFileSize == 0) { _maximumCacheFileSize = 5 * 1024 * 1024; }
                return _maximumCacheFileSize;
            }
            set => _maximumCacheFileSize = Util.Clamp(value, 1, Int64.MaxValue);
            
        }

        private string _staticFilesBaseDirectory;
        public string StaticFilesBaseDirectory
        {
            get
            {
                if(_staticFilesBaseDirectory == null) { _staticFilesBaseDirectory = ""; }
                return _staticFilesBaseDirectory;
            }
            set => _staticFilesBaseDirectory = value.TrimStart('/', '\\');
        }
    }
}
