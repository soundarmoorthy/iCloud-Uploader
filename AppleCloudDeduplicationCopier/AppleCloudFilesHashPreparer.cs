using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace Apple.Cloud.DeduplicationCopier
{
    public class AppleCloudDirectory
    {
        private static readonly string cacheFileName = "cache.csv";
        private readonly string basePath;

        public AppleCloudDirectory(string basePath)
        {
            this.basePath = basePath;
        }

        public Dictionary<string, CacheData> GetFilesWithHashes()
        {
            return CacheFileReader.Read(Path.GetFullPath(cacheFileName));
        }

        public ConcurrentDictionary<string, CacheData> PrepareHashes()
        {
            var entries = new ConcurrentDictionary<string, CacheData>();
            if (File.Exists(cacheFileName))
            {
                entries = new ConcurrentDictionary<string, CacheData>(GetFilesWithHashes().AsEnumerable());
            }
            var files = System.IO.Directory.EnumerateFiles(basePath, "*.*", SearchOption.AllDirectories)
                                           .OrderByDescending(x => File.GetLastWriteTime(x));
            Parallel.ForEach(files, (sourceFileName) =>
            {
                //If file is already hashed and available in the cache file, use it.
                var lastWriteTime = File.GetLastWriteTime(sourceFileName);
                if (entries.Values.Any(x => x.Path == sourceFileName))
                {
                    Log.Information($"File {sourceFileName} is already present.");
                    return;
                }
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(sourceFileName))
                    {
                        var hash = Convert.ToHexString(md5.ComputeHash(stream));
                        if (!entries.ContainsKey(hash))
                        {
                            entries.TryAdd(hash, new CacheData() { Hash = hash, Path = sourceFileName });
                            File.AppendAllText(cacheFileName, string.Format("{0},{1}{2}", hash, sourceFileName, Environment.NewLine));
                            var logMessage = string.Format("File {0} : Hash {1}", sourceFileName, hash);
                            Log.Information(logMessage);
                        }
                        else  // Existing Entry
                        {
                            var logMessage = string.Format("Duplicate File {0} with hash {1} exists. Original file {2}",
                                            sourceFileName, hash, entries[hash].Path);
                            Log.Information(logMessage);
                        }
                    }
                }
            });
            return entries;
        }
    }
}