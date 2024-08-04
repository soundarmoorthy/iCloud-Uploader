

using CsvHelper.Configuration.Attributes;
namespace Apple.Cloud.DeduplicationCopier
{
    public class CacheData
    {
        [Index(0)]
        public required string Hash { get; set; }

        [Index(1)]
        public required string Path { get; set; }

        // [Index(2)]
        // public DateTime ModifiedDateTime { get; set; }
    }
}