


using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Apple.Cloud.DeduplicationCopier
{
    public class CacheFileReader
    {
        public static Dictionary<string, CacheData> Read(string path)
        {
            if(!File.Exists(path))
                return new Dictionary<string, CacheData>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, config))
            {
                return csv.GetRecords<CacheData>().ToDictionary(x => x.Hash);
            }
        }
    }
}