using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace Alabaster
{
    public static class FileIO
    {
        private static ConcurrentDictionary<string, string> extensionPaths = new ConcurrentDictionary<string, string>(2, 100);
        private static LockableDictionary<IPath, bool> allowedPaths = new LockableDictionary<IPath, bool>(100);
        private static bool whitelistMode = false;
        private static volatile bool initialized = false;
        private static string staticBase = "";

        internal static void Init()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                FilePath path = new FilePath(asm.Location);
                allowedPaths[path] = false;
                allowedPaths.Lock(path);
            }
            initialized = true;
        }

        public static void AllowFiles(params string[] files) => Array.ForEach(files, (string f)=> AddPath(new FilePath(f), true));
        public static void ForbidFiles(params string[] files) => Array.ForEach(files, (string f)=> AddPath(new FilePath(f), false));
        public static void AllowDirectories(params string[] directories) => Array.ForEach(directories, (string d)=> AddPath(new DirectoryPath(d), true));
        public static void ForbidDirectories(params string[] directories) => Array.ForEach(directories, (string d)=> AddPath(new DirectoryPath(d), false));
        public static void SetWhitelistMode() => whitelistMode = true;
        public static void SetBlacklistMode() => whitelistMode = false;

        public static bool IsFileAllowed(string file) => IsPathAllowed(new FilePath(file));
        public static bool IsDirectoryAllowed(string directory) => IsPathAllowed(new DirectoryPath(directory));

        public static string StaticFilesBaseDirectory
        {
            get => staticBase;
            set => staticBase = value.TrimStart('/', '\\');                
        }

        public static void SetFileExtensionDirectory(string extension, string directory) => extensionPaths[extension] = directory;
        public static bool RemoveFileExtensionDirectory(string extension) => extensionPaths.TryRemove(extension, out _);
        public static string GetFileExtensionDirectory(string extension) => extensionPaths[extension];

        public static byte[] GetFile(string file) => LRUCache.GetStaticFileData(file, staticBase);
        public static byte[] GetFile(string file, string baseDirectory) => LRUCache.GetStaticFileData(file, baseDirectory);

        //private
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
        }

        private struct FilePath : IPath
        {
            public string Value;
            public FilePath(string val) => this.Value = val.Replace('\\', '/');
            public DirectoryPath GetDirectory() => new DirectoryPath(this.Value.Substring(0, this.Value.LastIndexOf('/')));
        }

        private struct DirectoryPath : IPath
        {
            public string Value;
            public DirectoryPath(string val)
            {
                this.Value = val.Replace('\\', '/');
                if (val == "" || val[val.Length - 1] != '/') { this.Value += "/"; }
            }
            public DirectoryPath GetDirectory() => this;
        }

        private static class LRUCache
        {
            private class FileData
            {
                private byte[] data;
                public LRUNode Node;

                public FileData() => this.Node = new LRUNode(this);

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
                public static object LRULock = new object();
                public static volatile int Count = 0;
                public static volatile LRUNode End = new LRUNode(null);
                public static volatile LRUNode NewestNode = End;
                public FileData data;
                public LRUNode Next;
                public LRUNode Previous;
                public LRUNode(FileData data) => this.data = data;
            }

            private static int CacheSize = 100;
            private static ConcurrentDictionary<string, FileData> fileDict = new ConcurrentDictionary<string, FileData>(Environment.ProcessorCount, CacheSize);
            
            public static byte[] GetStaticFileData(string file, string baseDir)
            {
                if(!initialized) { throw new InvalidOperationException("Server not yet initialized."); }
                string fullpath = String.Join("/", baseDir, file);
                FilePath f = new FilePath(fullpath);
                if (!IsPathAllowed(f) || !IsPathAllowed(f.GetDirectory()) || !File.Exists(fullpath)) { return null; }
                return (fileDict.TryGetValue(fullpath, out FileData result) == true) ? GetFromCache() : LoadFromDisk();

                byte[] LoadFromDisk()
                {
                    byte[] data = File.ReadAllBytes(fullpath);
                    if (result == null)
                    {
                        result = new FileData();
                        fileDict[fullpath] = result;
                    }
                    LRUPrepend();
                    result.Data = data;
                    return data;
                }

                byte[] GetFromCache()
                {
                    byte[] data = result?.Data;
                    return (data != null && File.GetLastWriteTime(fullpath) > result.Timestamp) ? data : LoadFromDisk();
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
                        toDelete.data.Data = null;
                        toDelete.Next = null;
                        toDelete.Previous = null;
                    }
                }
            }
        }
    }
}