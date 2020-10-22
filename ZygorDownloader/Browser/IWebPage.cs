using OpenQA.Selenium;

namespace ZygorDownloader.Browser
{
    public interface IWebPage<TWebDriver> where TWebDriver : IWebDriver
    {
        public string Url { get; }

        public void PreCheck(TWebDriver driver);

        public void PerformActions(TWebDriver driver);
        public void PostCheck(TWebDriver driver);
    }
}