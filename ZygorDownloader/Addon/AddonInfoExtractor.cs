using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GlobExpressions;
using MoonSharp.Interpreter;
using Serilog;
using Serilog.Core;
using ZygorDownloader.Ioc;
using ZygorDownloader.Mega;

namespace ZygorDownloader.Addon
{
    public interface IAddonInfoExtractor
    {
        AddonInfo Extract(DownloaderResult downloaderResult);
    }

    public class AddonInfoExtractor : IService, IAddonInfoExtractor
    {
        private readonly ILogger logger;

        public AddonInfoExtractor(ILogger logger)
        {
            this.logger = logger;
        }

        public AddonInfo Extract(DownloaderResult downloaderResult)
        {
            Dictionary<string, string> tocInfo;
            try
            {
                tocInfo = ExtractTocInfo(downloaderResult.Directory);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to extract addon's toc");
                tocInfo = new Dictionary<string, string>();
            }

            string revision;
            try
            {
                revision = ExtractRevision(downloaderResult.Directory);
            }
            catch (Exception e)
            {
                logger.Error(e, "Unable to extract addon's revision");
                revision = $"Unknown{downloaderResult.OriginalId}";
            }
            
            return new AddonInfo(revision, tocInfo, downloaderResult.OriginalId, downloaderResult.OriginalDate, downloaderResult.OriginalFileName);
        }

        private string ExtractRevision(string directory)
        {
            var versionFiles = new DirectoryInfo(directory).GlobFiles("**/Ver.lua").ToArray();
            if (versionFiles.Length > 1)
            {
                throw new Exception("More than 1 Ver.lua file found");
            }

            if (versionFiles.Length == 0)
            {
                throw new Exception("No Ver.lua file found");
            }

            var versionFile = versionFiles[0];

            using var f = versionFile.OpenText();
            var content = f.ReadToEnd();

            var lua = $@"
local tmp = {{}};
local ZygorGuidesViewer = tmp

local function GetAddOnMetadata(...)
    return """"
end

local function wrap (...)
{content}
end


wrap(""Zygor"", tmp)
return tmp.revision or tmp.version";

            var res = Script.RunString(lua);
            var revision = res.CastToString();
            logger.Debug("Found revision {0}", revision);
            return revision;
        }

        private Dictionary<string, string> ExtractTocInfo(string directory)
        {
            var tocArray = new DirectoryInfo(directory).GlobFiles("**/Zygor*.toc").ToArray();
            if (tocArray.Length > 1)
            {
                throw new Exception("More than 1 toc file found");
            }

            if (tocArray.Length == 0)
            {
                throw new Exception("No toc file found");
            }

            var toc = tocArray[0];

            var tocRegex = new Regex(@"^\s*##\s+(\w+[\w-]+)\s*:\s*(.*?)\s?$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);
            using var s = toc.OpenText();
            var tocInfo = new Dictionary<string, string>();
            while (!s.EndOfStream)
            {
                var l = s.ReadLine();
                if (l is null) break;
                var m = tocRegex.Match(l);
                if (!m.Success)
                {
                    continue;
                }

                if (m.Groups[1].Success && !string.IsNullOrWhiteSpace(m.Groups[1].Value) &&
                    m.Groups[2].Success && m.Groups[2].Value is {})
                {
                    tocInfo[m.Groups[1].Value.Trim()] = m.Groups[2].Value.Trim();
                }
            }

            logger.Debug("Found {0} toc items", tocInfo.Count);
            return tocInfo;
        }
    }
}