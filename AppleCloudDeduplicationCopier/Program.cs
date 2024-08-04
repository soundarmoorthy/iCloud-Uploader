using Serilog;

namespace Apple.Cloud.DeduplicationCopier
{
    public class Program
    {
        public static string iCloudPath = "<icloud path in local machine"; 
        // example : /Users/username/Library/Mobile Documents/com~apple~CloudDocs/
        public static int Main(String[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"log.txt",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
            var copier = new AppleCloudDeduplicationCopier();
            copier.Copy("<source-dir-to-copy>", iCloudPath);
            return 0;
        }
    }
}
