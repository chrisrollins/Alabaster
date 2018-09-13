﻿using System;
using System.Threading;

namespace Alabaster
{
    public struct ServerOptions
    {
        public bool EnableCustomHTTPMethods { get; set; }

        private int _schemesEnabled;
        public HTTPScheme SchemesEnabled
        {
            get
            {
                Interlocked.CompareExchange(ref this._schemesEnabled, (int)HTTPScheme.HTTP, 0);
                return (HTTPScheme)this._schemesEnabled;
            }
            set => this._schemesEnabled = (int)value;
        }

        private string _serverID;        
        public string ServerID
        {
            get
            {
                Interlocked.CompareExchange(ref this._serverID, Guid.NewGuid().ToString(), null);
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
                Interlocked.CompareExchange(ref this._maximumCacheFileSize, 5 * 1024 * 1024, 0);
                return _maximumCacheFileSize;
            }
            set => _maximumCacheFileSize = Util.Clamp(value, 1, Int64.MaxValue);            
        }

        private string _staticFilesBaseDirectory;
        public string StaticFilesBaseDirectory
        {
            get
            {
                Interlocked.CompareExchange(ref this._staticFilesBaseDirectory, "", null);
                return _staticFilesBaseDirectory;
            }
            set => _staticFilesBaseDirectory = value.TrimStart('/', '\\');
        }
    }
}
