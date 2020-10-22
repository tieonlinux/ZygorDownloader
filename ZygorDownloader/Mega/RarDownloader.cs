using System;
using System.IO;
using System.Linq;
using CG.Web.MegaApiClient;
using Serilog;
using SevenZipExtractor;
using ZygorDownloader.Ioc;

namespace ZygorDownloader.Mega
{
    public interface IRarDownloader
    {
        DownloaderResult DownloadFolder(Uri uri);
    }

    public class RarDownloader : IService, IRarDownloader
    {
        private readonly ILogger Logger;

        public RarDownloader(ILogger logger)
        {
            Logger = logger;
        }

        public DownloaderResult DownloadFolder(Uri uri)
        {
            var client = new MegaApiClient();
            client.LoginAnonymous();

            Logger.Debug("Successfully log into mega");
            try
            {
                var nodes = client.GetNodesFromLink(uri)
                    .Where(x => x.Type == NodeType.File && x.Name.EndsWith(".rar"))
                    .ToArray();
                if (nodes.Length > 1)
                {
                    throw new Exception("There's more that 1 rar file on the mega folder");
                }
                else if (nodes.Length <= 0)
                {
                    throw new Exception("There's no rar in the remote mega folder");
                }
                
                Logger.Debug("Found a rar file in {0}", uri);

                var node = nodes[0];


                var path = Path.GetTempFileName();
                Logger.Debug("Downloading {0} into {1}", node.Name, path);
                try
                {
                    using var file = File.Open(path, FileMode.Truncate);

                    {
                        using var downloadStream = client.Download(node);
                        downloadStream.CopyTo(file);
                    }

                    file.Seek(0, 0);
                    using var rar = new ArchiveFile(file);
                    var dir = path + ".extracted";
                    Logger.Debug("Extracting into {0}", dir);
                    Directory.CreateDirectory(dir);
                    try
                    {
                        rar.Extract(dir);
                        return new DownloaderResult(dir, node.Name, node.Id, node.Owner, node.ModificationDate ?? node.CreationDate);
                    }
                    catch
                    {
                        Logger.Warning("Deleting {0} before throwing", dir);
                        Directory.Delete(dir, true);
                        throw;
                    }
                }
                finally
                {
                    Logger.Debug("Removing temporary file {0}", path);
                    File.Delete(path);
                }
            }
            finally
            {
                client.Logout();
            }
        }
    }
}