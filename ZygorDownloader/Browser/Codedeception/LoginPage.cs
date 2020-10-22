using System;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Serilog;
using ZygorDownloader.Ioc;

namespace ZygorDownloader.Browser.Codedeception
{
    public class LoginPage : BaseWebPage, IService
    {
        private readonly ILogger Logger;
        private readonly IConfiguration configuration;

        public override string Url => "https://codedeception.net/index.php?/login/";

        public override void PreCheck(IWebDriver driver)
        {
            FindElements(driver);
        }

        public override void PostCheck(IWebDriver driver)
        {
            if (driver.Url != "https://codedeception.net/" && !driver.Url.EndsWith("?_fromLogin=1"))
            {
                Logger.Warning(
                    $"Url ({driver.Url}) doesn't ends with expected pattern, it may be due to login issue or remote site update");
            }

            if (driver.PageSource.Contains("Guest!"))
            {
                Logger.Error($"Current page still contains the Guest Tag");
                throw new Exception("Unable to log in");
            }
        }

        public LoginPage(ILogger logger, IConfiguration configuration)
        {
            Logger = logger;
            this.configuration = configuration;
        }

        private static (IWebElement login, IWebElement password) FindElements(ISearchContext driver)
        {
            var loginField = driver.FindElement(By.Id("elInput_auth"));
            if (loginField is null)
            {
                throw new Exception("Unable to locate the login form");
            }
            var passwordField = driver.FindElement(By.Id("elInput_password"));
            if (passwordField is null)
            {
                throw new Exception("Unable to locate the password form");
            }
            return (loginField, passwordField);
        }

        public override void PerformActions(IWebDriver driver)
        {
            var (loginField, passwordField) = FindElements(driver);
            
            SubmitLoginForm(loginField, passwordField);
        }

        private void SubmitLoginForm(IWebElement loginField, IWebElement passwordField)
        {
            var config = configuration.GetSection("Codedeception");
            loginField.SendKeys(config.GetValue<string>("Login"));

            passwordField.SendKeys(config.GetValue<string>("Password"));
            passwordField.Submit();
        }
    }
}