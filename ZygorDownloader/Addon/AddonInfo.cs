using System;
using System.Collections.Generic;

namespace ZygorDownloader.Addon
{
    public class AddonInfo
    {
        public readonly string Revision;
        public readonly Dictionary<string, string> Toc;
        public readonly string Id;
        public readonly DateTime Date;
        public readonly string File;

        public AddonInfo(string revision, Dictionary<string, string> toc, string id, DateTime date, string file)
        {
            Revision = revision;
            Toc = toc;
            Id = id;
            Date = date;
            File = file;
        }
    }
}