using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using SimpleInjector;
using ZygorDownloader.Addon;
using ZygorDownloader.Browser;
using ZygorDownloader.Browser.Codedeception;
using ZygorDownloader.Ioc;
using ZygorDownloader.Mega;

namespace ZygorDownloader
{
    class Program
    {
        static readonly Container container;
        static Program()
        {
            container = new Container();
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables(prefix: "ZY_")
                .AddJsonFile("overrides.json", true)
                .Build();

            var serilogConfig = new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(configuration);

            var logger = serilogConfig.CreateLogger();

            container.Options.DependencyInjectionBehavior = 
                new SerilogContextualLoggerInjectionBehavior(container.Options, logger);
            
            container.RegisterInstance(typeof(IServiceProvider), container);
            container.RegisterInstance(typeof(Logger), logger);
            container.RegisterInstance(typeof(LoggerConfiguration), serilogConfig);
            container.RegisterInstance(configuration.GetType(), configuration);
            container.RegisterInstance(typeof(IConfiguration), configuration);
            container.RegisterInstance(typeof(IConfigurationRoot), configuration);
            
            var registrations =
                from type in Assembly.GetCallingAssembly().GetExportedTypes()
                where type.Namespace?.StartsWith("ZygorDownloader.") ?? false
                where type.GetInterfaces().Contains(typeof(IService))
                from service in type.GetInterfaces()
                select new { service, implementation = type };

            container.Options.AllowOverridingRegistrations = true;
            foreach (var reg in registrations)
            {
                container.Register(reg.service, reg.implementation, Lifestyle.Transient);
                container.Register(reg.implementation, reg.implementation, Lifestyle.Transient);
            }
            container.Options.AllowOverridingRegistrations = false;
            
            container.Verify();
        }

        
        static void Main(string[] args)
        {
            var session = container.GetInstance<ISession>();
            var links = session.CollectZygorLinks();
            if (!Directory.Exists("export"))
            {
                Directory.CreateDirectory("export");
            }

            foreach (var link in new []{links.ZygorUrl, links.ZygorClassicUrl})
            {
                using var res = container.GetInstance<IRarDownloader>().DownloadFolder(link);
                container.GetInstance<AddonPacker>().Pack(res);
            }
            
        }
    }
}