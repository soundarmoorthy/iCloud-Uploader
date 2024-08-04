
using System.Collections.Concurrent;
using System.Security.Cryptography;
using Serilog;

namespace Apple.Cloud.DeduplicationCopier
{
    public class AppleCloudDeduplicationCopier
    {
        public void Copy(string sourceDir, string iCloudPath)
        {
            // This is long running depending on the number and size of files in iCloud Drive
            // While it's parallel to the maximum instant, it's still limited by disk IO, 
            // cached content in disk versus content to be downloaded from iCloud server.
            var dict = new AppleCloudDirectory(iCloudPath).PrepareHashes();


            // This is also time consuming depending on how much files are being hashed 
            // from the source directory.
            var newFiles = GetFilesToCopyWithHashes(sourceDir);
            foreach (var newFileHash in newFiles.Keys)
            {
                var newFilePath = newFiles[newFileHash];
                if (dict.ContainsKey(newFileHash))
                {
                    Console.WriteLine($"File {newFilePath} exists in ICloud Drive with Hash {newFileHash} and will be deleted in source.");
                    File.Delete(newFilePath);
                }
                else
                {
                    var destFileName = Path.GetFileName(newFilePath);
                    var destFilePath = Path.Combine(iCloudPath, destFileName);
                    if (File.Exists(destFilePath))
                        destFilePath = Path.Combine(iCloudPath, DateTime.Now.ToFileTime().ToString() + "-" + destFileName);

                    File.Copy(newFilePath, destFilePath);
                    Log.Information($"Copied {newFilePath} to {destFilePath}");
                    File.AppendAllText("cache.csv", string.Format("{0},{1}{2}", newFileHash, destFilePath, Environment.NewLine));
                }
            }
        }

        public static ConcurrentDictionary<string, string> GetFilesToCopyWithHashes(string path)
        {
            var entries = new ConcurrentDictionary<string, string>();
            var files = System.IO.Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
            Parallel.ForEach(files, (sourceFileName) =>
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(sourceFileName))
                    {
                        var hash = Convert.ToHexString(md5.ComputeHash(stream));
                        if (!entries.ContainsKey(hash))
                        {
                            entries.TryAdd(hash, sourceFileName);
                            var logMessage = string.Format("[{0}][Thread {1}] File {2} : Hash {3}{4}", DateTime.Now,
                                            Thread.CurrentThread.ManagedThreadId, sourceFileName, hash, Environment.NewLine);
                            Console.Write(logMessage);
                            Log.Information(logMessage);
                        }
                        else  // Existing Entry
                        {
                            var logMessage = string.Format("[{0}][Thread {1}] Skipping file {2} as hash {3} exists {4}", DateTime.Now,
                                            Thread.CurrentThread.ManagedThreadId, sourceFileName, hash, Environment.NewLine);
                            Console.Write(logMessage);
                            Log.Information(logMessage);
                        }
                    }
                }
            });
            return entries;
        }
    }
}