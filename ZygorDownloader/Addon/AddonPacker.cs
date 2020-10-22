using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.PortableExecutable;
using GlobExpressions;
using Newtonsoft.Json;
using Serilog;
using ZygorDownloader.Ioc;
using ZygorDownloader.Mega;

namespace ZygorDownloader.Addon
{
    public class AddonPacker : IService
    {
        public readonly ILogger Logger;
        public IAddonInfoExtractor AddonInfoExtractor;

        public AddonPacker(ILogger logger, IAddonInfoExtractor addonInfoExtractor)
        {
            Logger = logger;
            AddonInfoExtractor = addonInfoExtractor;
        }

        public void Pack(DownloaderResult downloaderResult)
        {
            var directory = downloaderResult.Directory;
            var addonInfo = AddonInfoExtractor.Extract(downloaderResult);
            var addonName = new DirectoryInfo(directory).GlobDirectories("*").First().Name;
            var target = Path.Join("export", $"{addonName}.zip");
            if (File.Exists(target))
            {
                Logger.Information("Removing {0} in order to recreate anew", target);
                File.Delete(target);
            }
            ZipFile.CreateFromDirectory(Path.Join(directory, addonName), target, CompressionLevel.Fastest, true);
            using var f = File.CreateText(Path.Join("export", $"{addonName}.json"));
            f.Write(JsonConvert.SerializeObject(addonInfo, Formatting.Indented));
            Logger.Information("Updated {0} to {1}", target, addonInfo.Revision);
        }
    }
}