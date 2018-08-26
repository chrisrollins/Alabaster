using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Alabaster
{
    public static class FileIO
    {
        private static readonly ConcurrentDictionary<string, string> extensionPaths = new ConcurrentDictionary<string, string>(2, 100);
        private static readonly LockableDictionary<IPath, bool> allowedPaths = new LockableDictionary<IPath, bool>(100);
        private static bool whitelistMode = false;
        private static volatile bool initialized = false;
        
        static FileIO()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                FilePath path = new FilePath(asm.Location);
                allowedPaths[path] = false;
                allowedPaths.Lock(path);
            }
        }

        public static void InitializeFileRequestHandler()
        {
            ServerThreadManager.Run(() =>
            {
                if(initialized) { return; }
                initialized = true;
                Server.All((Request req) => (Util.GetFileExtension(req.Route) != null) ? (Response)GetFile(req.Route) : new PassThrough());
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
        public static string GetFileExtensionDirectory(string extension) => extensionPaths[extension];

        public static FileData GetFile(string file) => new FileData(LRUCache.GetStaticFileData((FilePath)file, (DirectoryPath)Server.Config.StaticFilesBaseDirectory));
        public static FileData GetFile(string file, string baseDirectory) => new FileData(LRUCache.GetStaticFileData((FilePath)file, (DirectoryPath)baseDirectory));
        public static async Task<FileData> GetFileAsync(string file) => await new Task<FileData>(() => GetFile(file));
        public static async Task<FileData> GetFileAsync(string file, string baseDirectory) => await new Task<FileData>(() => GetFile(file, baseDirectory));
        
        public struct FileData
        {
            internal readonly byte[] Data;
            public byte this[int i] { get => this.Data[i]; }
            internal FileData(byte[] data) => this.Data = data;
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
            string Value { get; set; }
        }

        private struct FilePath : IPath
        {
            public string Value { get; set; }
            public FilePath(string val) => this.Value = val.Replace('\\', '/');
            public DirectoryPath GetDirectory() => new DirectoryPath(this.Value.Substring(0, Util.Clamp(this.Value.LastIndexOf('/'), 0, int.MaxValue)));
            public static explicit operator FilePath(string path) => new FilePath(path);
        }

        private struct DirectoryPath : IPath
        {
            public string Value { get; set; }
            public DirectoryPath(string val)
            {
                this.Value = val.Replace('\\', '/');
                if (val == "" || val[val.Length - 1] != '/') { this.Value += "/"; }
            }
            public DirectoryPath GetDirectory() => this;
            public static IPath operator +(DirectoryPath p1, IPath p2)
            {
                string value = p1.Value + p2.Value;
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
                public static object LRULock = new object();
                public static volatile int Count = 0;
                public static volatile LRUNode End = new LRUNode(null);
                public static volatile LRUNode NewestNode = End;
                public CachedFile file;
                public LRUNode Next;
                public LRUNode Previous;
                public LRUNode(CachedFile data) => this.file = data;
            }

            private static int CacheSize = 100;
            private static ConcurrentDictionary<FilePath, CachedFile> fileDict = new ConcurrentDictionary<FilePath, CachedFile>(Environment.ProcessorCount, CacheSize);
            
            public static byte[] GetStaticFileData(FilePath file, DirectoryPath baseDir)
            {
                if(!initialized) { throw new InvalidOperationException("Server not yet initialized."); }
                FilePath fullPath = (FilePath)(baseDir + file);
                if (!IsFileValid(fullPath)) { return null; }
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
                    return (data != null && File.GetLastWriteTime(fullPath.Value) > result.Timestamp) ? data : LoadFromDisk();
                }

                bool IsFileValid(FilePath filePathValid) => IsPathAllowed(filePathValid) && IsPathAllowed(filePathValid.GetDirectory()) && File.Exists(fullPath.Value);

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