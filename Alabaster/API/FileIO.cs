﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Alabaster
{
    public static class FileIO
    {
        private static readonly ConcurrentDictionary<string, string> extensionPaths = new ConcurrentDictionary<string, string>(2, 100);
        private static readonly LockableDictionary<IPath, bool> allowedPaths = new LockableDictionary<IPath, bool>(100);
        private static bool whitelistMode = false;
        private static long initialized = 0;
        
        static FileIO()
        {
            Array.ForEach(AppDomain.CurrentDomain.GetAssemblies(), asm =>
            {
                FilePath path = new FilePath(asm.Location);
                allowedPaths[path] = false;
                allowedPaths.Lock(path);
            });
        }

        public static void InitializeFileRequestHandler()
        {
            if(Interlocked.CompareExchange(ref initialized, 1, 0) == 1) { return; }            
            Routing.AddHandlerInternal((MethodArg)null, (RouteArg)null, (Request req) =>
            {
                if(Util.GetFileExtension(req.Route) == null) { return PassThrough.Default; }
                FileData file = GetFile(req.Route);
                return (file.Data != null) ? (Response)file : new PassThrough(null, HTTPStatus.NotFound);
            });            
        }

        public static void AllowFiles(params string[] files) => Array.ForEach(files, (string f) => AddPath(new FilePath(f), true));
        public static void ForbidFiles(params string[] files) => Array.ForEach(files, (string f) => AddPath(new FilePath(f), false));
        public static void AllowDirectories(params string[] directories) => Array.ForEach(directories, (string d) => AddPath(new DirectoryPath(d), true));
        public static void ForbidDirectories(params string[] directories) => Array.ForEach(directories, (string d) => AddPath(new DirectoryPath(d), false));
        public static void SetWhitelistMode() => whitelistMode = true;
        public static void SetBlacklistMode() => whitelistMode = false;

        public static bool IsFileAllowed(string file) => IsPathAllowed(new FilePath(file));
        public static bool IsDirectoryAllowed(string directory) => IsPathAllowed(new DirectoryPath(directory));

        public static void SetFileExtensionDirectory(string extension, string directory) => extensionPaths[extension] = directory;
        public static bool RemoveFileExtensionDirectory(string extension) => extensionPaths.TryRemove(extension, out _);
        public static string GetFileExtensionDirectory(string extension) => extensionPaths.TryGetValue(extension, out string path) ? path : "";

        public static FileData GetFile(string file) => new FileData(file, Server.Config.StaticFilesBaseDirectory);
        public static FileData GetFile(string file, string baseDirectory) => new FileData(file, baseDirectory);
        public static async Task<FileData> GetFileAsync(string file) => await new Task<FileData>(() => new FileData(file, Server.Config.StaticFilesBaseDirectory, true));
        public static async Task<FileData> GetFileAsync(string file, string baseDirectory) => await new Task<FileData>(() => new FileData(file, baseDirectory, true));

        public struct FileData
        {
            private byte[] data;
            public byte[] Data
            {
                get
                {
                    if (this.data == null) { this.data = LRUCache.GetStaticFileData(this.filePath, this.baseDir); }
                    return this.data;
                }
            }
            public bool Found => ((FilePath)this.FullPath).Valid;
            public byte this[int i] => this.Data[i];
            private DirectoryPath baseDir;
            private FilePath filePath;
            public string BaseDirectory => this.baseDir.Value;
            public string FilePath => this.filePath.Value;
            public string FullPath => (this.baseDir + this.filePath).Value;
            internal FileData(string path, string baseDir, bool preload = false)
            {
                this.filePath = (FilePath)path;
                this.baseDir = (DirectoryPath)baseDir;
                this.data = null;
                if (preload) { _ = this.Data; }
            }
            internal FileData(byte[] data)
            {
                this.data = data;
                this.baseDir = (DirectoryPath)"";
                this.filePath = (FilePath)"";
            }
        }

        private static void AddPath(IPath p, bool allowed) => allowedPaths[p] = allowed;
        private static bool IsPathAllowed(IPath p)
        {
            bool inDict = allowedPaths.TryGetValue(p, out bool allowed);
            allowed = (inDict) ? allowed : !whitelistMode;
            return (p is FilePath) ? IsPathAllowed(p.GetDirectory()) : allowed;
        }
        
        private interface IPath
        {
            DirectoryPath GetDirectory();
            string GetExtension();
            string Value { get; set; }
        }

        private struct FilePath : IPath
        {
            public string Value { get; set; }
            public FilePath(string val) => this.Value = val.Replace('\\', '/');
            public DirectoryPath GetDirectory() => new DirectoryPath(this.Value.Substring(0, Util.Clamp(this.Value.LastIndexOf('/'), 0, int.MaxValue)));
            public static explicit operator FilePath(string path) => new FilePath(path);
            public bool Valid => IsPathAllowed(this) && IsPathAllowed(this.GetDirectory()) && File.Exists(this.Value);
            public string GetExtension() => Util.GetFileExtension(this.Value) ?? "";
        }

        private struct DirectoryPath : IPath
        {
            public string Value { get; set; }
            public DirectoryPath(string val)
            {
                this.Value = val.Replace('\\', '/');
            }
            public DirectoryPath GetDirectory() => this;
            public string GetExtension() => "";
            public static IPath operator +(DirectoryPath p1, IPath p2)
            {
                string dir = p1.Value;
                if(!string.IsNullOrEmpty(dir) && dir[dir.Length - 1] != '/') { dir = dir + "/"; }
                string extDir = GetFileExtensionDirectory(p2.GetExtension());
                if(!string.IsNullOrEmpty(extDir)) { extDir += "/"; }
                string value = string.Join(null, dir, extDir, p2.Value);
                return (p2.GetType() == typeof(DirectoryPath)) ? (IPath)new DirectoryPath(value) : new FilePath(value);
            }
            public static explicit operator DirectoryPath(string path) => new DirectoryPath(path);
        }

        private static class LRUCache
        {
            private class CachedFile
            {
                private byte[] data;
                public LRUNode Node;
                public CachedFile() => this.Node = new LRUNode(this);
                public DateTime Timestamp { get; private set; }
                public byte[] Data
                {
                    get => data;
                    set
                    {
                        Timestamp = DateTime.Now;
                        data = value;
                    }
                }
            }

            private class LRUNode
            {
                internal static object LRULock = new object();
                internal static volatile int Count = 0;
                internal static volatile LRUNode End = new LRUNode(null);
                internal static volatile LRUNode NewestNode = End;
                internal CachedFile file;
                internal LRUNode Next;
                internal LRUNode Previous;
                internal LRUNode(CachedFile data) => this.file = data;
            }

            private static int CacheSize = 100;
            private static ConcurrentDictionary<FilePath, CachedFile> fileDict = new ConcurrentDictionary<FilePath, CachedFile>(Environment.ProcessorCount, CacheSize);
            
            internal static byte[] GetStaticFileData(FilePath file, DirectoryPath baseDir)
            {
                FilePath fullPath = (FilePath)(baseDir + file);
                if (!fullPath.Valid) { return null; }
                return (fileDict.TryGetValue(fullPath, out CachedFile result) == true) ? GetFromCache() : LoadFromDisk();

                byte[] LoadFromDisk()
                {
                    byte[] data = File.ReadAllBytes(fullPath.Value);
                    if(data.LongLength > Server.Config.MaximumCacheFileSize) { return data; }
                    if (result == null)
                    {
                        result = new CachedFile();
                        fileDict[fullPath] = result;
                    }
                    LRUPrepend();
                    result.Data = data;
                    return data;
                }

                byte[] GetFromCache()
                {
                    byte[] data = result?.Data;
                    return (data != null && File.GetLastWriteTime(fullPath.Value) < result.Timestamp) ? data : LoadFromDisk();
                }

                void LRUPrepend()
                {
                    lock (LRUNode.LRULock)
                    {
                        //already in the list
                        if (result.Node.Next != null)
                        {
                            result.Node.Previous.Next = result.Node.Next;
                            result.Node.Next.Previous = result.Node;
                            result.Node.Previous = null;
                        }
                        else
                        {
                            LRUNode.Count++;
                        }

                        //prepend the node
                        result.Node.Next = LRUNode.NewestNode;
                        LRUNode.NewestNode.Previous = result.Node;
                        LRUNode.NewestNode = result.Node;

                        //check if there are too many nodes and remove the last one if there are.
                        //leaves the dict entry with the ref to the node, but deletes the byte array. so the dict entry can be reused by simply adding a new byte array and relinking it into the list
                        if (LRUNode.Count <= CacheSize) { return; }
                        LRUNode toDelete = LRUNode.End.Previous;
                        LRUNode.End.Previous = toDelete.Previous;
                        LRUNode.End.Previous.Next = LRUNode.End;
                        toDelete.file.Data = null;
                        toDelete.Next = null;
                        toDelete.Previous = null;
                    }
                }
            }
        }
    }
}