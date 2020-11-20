using System;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using Serilog;
using ZygorDownloader.Ioc;

namespace ZygorDownloader.Browser.Codedeception
{
    public class ZygorPage : BaseWebPage, IService
    {
        private readonly ILogger Logger;

        public ZygorPage(ILogger logger)
        {
            Logger = logger;
        }

        public override string Url => "https://codedeception.net/index.php?/topic/14175-zygor-guides-bfa-classic/&page=1";

        public ZygorPageResult? Result;
        
        public override void PerformActions(IWebDriver driver)
        {
            var source = driver.PageSource;

            var retail = ExtractZygorLink(source);
            var classic = ExtractZygorClassicLink(source);
            
            Result = new ZygorPageResult(retail, classic);
        }

        private static Uri ExtractZygorLink(string source)
        {
            var retailRegex = new Regex(@"Zygor\s+Retail\s+\|\s+.+?""\s*(https:\/\/.*mega.nz.*?)\s*""", RegexOptions.IgnoreCase);
            var m = retailRegex.Match(source);
            if (!m.Success)
            {
                throw new Exception("Unable to find latest zygor retail link");
            }

            var url = m.Groups[1].Value;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new Exception("The latest zygor retail link is not valid");
            }

            return uri;
        }
        
        private static Uri ExtractZygorClassicLink(string source)
        {
            var classicRegex = new Regex(@"Zygor\s+Classic\s+\|\s+.+?""\s*(https:\/\/.*?mega.nz.*?)\s*""", RegexOptions.IgnoreCase);
            var m = classicRegex.Match(source);
            if (!m.Success)
            {
                throw new Exception("Unable to find latest zygor classic link");
            }

            var url = m.Groups[1].Value;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new Exception("The latest zygor classic link is not valid");
            }

            return uri;
        }
    }

    public class ZygorPageResult
    {
        public readonly Uri ZygorUrl;
        public readonly Uri ZygorClassicUrl;

        public ZygorPageResult(Uri zygorUrl, Uri zygorClassicUrl)
        {
            ZygorUrl = zygorUrl;
            ZygorClassicUrl = zygorClassicUrl;
        }
    }
}