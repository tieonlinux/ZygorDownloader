using System;

namespace ZygorDownloader.Mega
{
    public class DownloaderResult : IDisposable
    {
        public readonly string Directory;
        public readonly string OriginalFileName;
        public readonly string OriginalId;
        public readonly string OriginalOwner;
        public readonly DateTime OriginalDate;

        public DownloaderResult(string directory, string originalFileName, string originalId, string originalOwner, DateTime originalDate)
        {
            Directory = directory;
            OriginalFileName = originalFileName;
            OriginalId = originalId;
            OriginalOwner = originalOwner;
            OriginalDate = originalDate;
        }

        public void Dispose()
        {
            System.IO.Directory.Delete(Directory, true);
        }
    }
}