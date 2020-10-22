using System;
using Serilog;
using Serilog.Core;
using SimpleInjector;
using SimpleInjector.Advanced;

namespace ZygorDownloader.Ioc
{
    public class SerilogContextualLoggerInjectionBehavior : IDependencyInjectionBehavior
    {
        private readonly IDependencyInjectionBehavior original;
        private readonly Container container;
        private readonly Logger logger;

        public SerilogContextualLoggerInjectionBehavior(ContainerOptions options, Logger logger)
        {
            this.logger = logger;
            original = options.DependencyInjectionBehavior;
            container = options.Container;
        }

        public void Verify(InjectionConsumerInfo consumer) => original.Verify(consumer);

        public bool VerifyDependency(InjectionConsumerInfo dependency, out string? errorMessage)
        {
            return original.VerifyDependency(dependency, out errorMessage);
        }

        public InstanceProducer? GetInstanceProducer(InjectionConsumerInfo i, bool t) =>
            i.Target.TargetType == typeof(ILogger)
                ? GetLoggerInstanceProducer(i.ImplementationType)
                : original.GetInstanceProducer(i, t);

        private InstanceProducer<ILogger> GetLoggerInstanceProducer(Type type) =>
            Lifestyle.Singleton.CreateProducer(
                () => logger.ForContext(type),
                container);
        
    }
}