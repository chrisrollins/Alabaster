using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Alabaster
{
    public static class FileIO
    {
        private static ConcurrentDictionary<string, string> extensionPaths = new ConcurrentDictionary<string, string>(2, 100);
        private static Dictionary<Path, bool> allowedPaths = new Dictionary<Path, bool>(100);
        private static string serverExePath = Environment.GetCommandLineArgs()[0];
        private static bool whitelistMode = false;

        public static void AllowFile(string file) => AddPath(new FilePath(file), true);
        public static void ForbidFile(string file) => AddPath(new FilePath(file), false);
        public static void AllowDirectory(string directory) => AddPath(new DirectoryPath(directory), true);
        public static void ForbidDirectory(string directory) => AddPath(new DirectoryPath(directory), false);
        public static void SetWhitelistMode() => whitelistMode = true;
        public static void SetBlacklistMode() => whitelistMode = false;

        public static bool IsFileAllowed(string file) => IsPathAllowed(new FilePath(file));
        public static bool IsDirectoryAllowed(string directory) => IsPathAllowed(new DirectoryPath(directory));

        public static string StaticFilesBaseDirectory = "";
        public static void SetFileExtensionDirectory(string extension, string directory) => extensionPaths[extension] = directory;
        public static bool RemoveFileExtensionDirectory(string extension) => extensionPaths.TryRemove(extension, out string junk);
        public static string GetFileExtensionDirectory(string extension) => extensionPaths[extension];

        public static byte[] GetStaticFile(string file) => LRUCache.GetStaticFileData(file);

        //private
        private static void AddPath(Path p, bool allowed) => allowedPaths[p] = allowed;
        private static bool IsPathAllowed(Path p)
        {
            bool inDict = allowedPaths.TryGetValue(p, out bool allowed);
            allowed = (inDict) ? allowed : !whitelistMode;
            return (p is FilePath) ? IsPathAllowed((p as FilePath).Directory) : allowed;
        }

        private abstract class Path { public Path(string val) => this.Value = val; public string Value; }
        private sealed class FilePath : Path
        {
            public FilePath(string val) : base(val) { }
            public DirectoryPath Directory
            {
                get
                {
                    return new DirectoryPath(this.Value.Substring(0, this.Value.LastIndexOf('\\')));
                }
            }
        }
        private sealed class DirectoryPath : Path
        {
            public DirectoryPath(string val) : base(val)
            {
                if (val[val.Length - 1] != '\\') { this.Value += "\\"; }
            }
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

            public static byte[] GetStaticFileData(string file)
            {
                string fullpath = String.Join("\\", StaticFilesBaseDirectory, file);
                FilePath f = new FilePath(fullpath);
                if (!IsPathAllowed(f) || !IsPathAllowed(f.Directory) || !File.Exists(fullpath)) { return null; }
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