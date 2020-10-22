using System;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using ZygorDownloader.Ioc;

namespace ZygorDownloader.Browser.Codedeception
{
    public interface ISession
    {
        ZygorPageResult CollectZygorLinks();
    }

    public class Session : IService, ISession
    {
        private readonly WebDriverFactory _webDriverFactory;
        private readonly IServiceProvider _serviceProvider;

        public Session(IServiceProvider serviceProvider, WebDriverFactory webDriverFactory)
        {
            _webDriverFactory = webDriverFactory;
            _serviceProvider = serviceProvider;
        }

        public ZygorPageResult CollectZygorLinks()
        {
            using var driver = _webDriverFactory.Create();
            
            var loginPage = _serviceProvider.GetService<LoginPage>();
            PerformPageActions(driver, loginPage);
            
            var zygorPage = _serviceProvider.GetService<ZygorPage>();
            PerformPageActions(driver, zygorPage);
            if (zygorPage.Result != null)
            {
                return zygorPage.Result;
            }
            throw new Exception("zygorPage.Result is null");
        }

        private static void PerformPageActions<TDriver>(TDriver driver, IWebPage<TDriver> page) where TDriver : IWebDriver
        {
            driver.Navigate().GoToUrl(page.Url);
            page.PreCheck(driver);
            page.PerformActions(driver);
            page.PostCheck(driver);
        }
    }
}