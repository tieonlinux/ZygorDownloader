using OpenQA.Selenium;

namespace ZygorDownloader.Browser
{
    public abstract class BaseWebPage : IWebPage<IWebDriver>
    {
        public abstract string Url { get; }

        public virtual void PreCheck(IWebDriver driver)
        {
        }

        public abstract void PerformActions(IWebDriver driver);

        public virtual void PostCheck(IWebDriver driver)
        {
        }
    }
}