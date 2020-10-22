using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ZygorDownloader.Ioc;

namespace ZygorDownloader.Browser
{
    public interface IWebDriverFactory
    {
        IWebDriver Create();
    }

    public class WebDriverFactory : IWebDriverFactory, IService
    {
        private readonly IConfigurationSection _chromeConfig;

        public WebDriverFactory(IConfiguration configuration)
        {
            _chromeConfig = configuration.GetSection("Selenium:Chrome");
        }

        public IWebDriver Create()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var options = new ChromeOptions();
            
#if DEBUG
            options.LeaveBrowserRunning = true;
#else
            HeadlessMode(options);
#endif
            InjectConfiguration(options);
            DisableImageLoading(options);
            return new ChromeDriver(options);
        }

        private void InjectConfiguration(ChromeOptions options)
        {
            var arguments = _chromeConfig.GetValue("Arguments", new List<string>());
            if (arguments.Any())
            {
                options.AddArguments(arguments.ToArray());
            }
        }
        
        private void HeadlessMode(ChromeOptions options)
        {
            if (!_chromeConfig.GetValue("Headless", true)) return;
            if (options.Arguments.Contains("--headless")) return;
            options.AddArguments("--headless", "--disable-extensions", "--disable-gpu");
        }
        
        private void DisableImageLoading(ChromeOptions options)
        {
            if (!_chromeConfig.GetValue("DisableImageLoading", true)) return;
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
        }
    }
}