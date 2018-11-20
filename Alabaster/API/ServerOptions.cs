using System;
using System.Threading;

namespace Alabaster
{
    public struct ServerOptions
    {
        public static class Defaults
        {
            public const bool DropUnknownCookies = false;
            public const bool EnableCustomHTTPMethods = false;
            public const bool EnableRouteDiagnostics = false;
            public const HTTPScheme SchemesEnabled = HTTPScheme.HTTP;
            public static readonly string ServerID = Guid.NewGuid().ToString();
            public const Int64 MaximumCacheFileSize = 5 * 1024 * 1024;
            public const string StaticFilesBaseDirectory = "";
        }

        public bool DropUnknownCookies { get; set; }
        public bool EnableCustomHTTPMethods { get; set; }
        public bool EnableRouteDiagnostics { get; set; }

        private int _schemesEnabled;
        public HTTPScheme SchemesEnabled
        {
            get
            {
                Interlocked.CompareExchange(ref this._schemesEnabled, (int)Defaults.SchemesEnabled, 0);
                return (HTTPScheme)this._schemesEnabled;
            }
            set => this._schemesEnabled = (int)value;
        }

        private string _serverID;        
        public string ServerID
        {
            get
            {
                Interlocked.CompareExchange(ref this._serverID, Defaults.ServerID, null);
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
                Interlocked.CompareExchange(ref this._maximumCacheFileSize, Defaults.MaximumCacheFileSize, 0);
                return _maximumCacheFileSize;
            }
            set => _maximumCacheFileSize = Util.Clamp(value, 1, Int64.MaxValue);            
        }

        private string _staticFilesBaseDirectory;
        public string StaticFilesBaseDirectory
        {
            get
            {
                Interlocked.CompareExchange(ref this._staticFilesBaseDirectory, Defaults.StaticFilesBaseDirectory, null);
                return _staticFilesBaseDirectory;
            }
            set => _staticFilesBaseDirectory = value.TrimStart('/', '\\');
        }
    }
}
