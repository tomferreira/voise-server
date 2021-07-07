using Autofac;
using log4net;
using log4net.Config;
using System;
using System.IO;
using Topshelf;
using Topshelf.Autofac;

namespace VoiseService
{
    static class Program
    {
        static void Main()
        {
            // This will ensure that future calls to Directory.GetCurrentDirectory()
            // returns the actual executable directory and not something like C:\Windows\System32 
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Specify the base name, display name and description for the service, as it is registered in the services control manager.
            // This information is visible through the Windows Service Monitor
            const string serviceName = "Voise Server";
            const string displayName = "Voise Server Service";
            const string description = "A .NET Windows Service for Voise Server.";

            HostFactory.Run(x =>
            {
                x.UseLog4Net();

                var container = CreateContainer();

                x.UseAutofacContainer(container);

                x.Service<Service.WinService>(sc =>
                {
                    sc.ConstructUsingAutofacContainer();

                    // the start and stop methods for the service
                    sc.WhenStarted((s, hostControl) => s.Start(container));
                    sc.WhenStopped((s, hostControl) => s.Stop());

                    // optional, when shutdown is supported
                    sc.WhenShutdown((s, hostControl) => s.Shutdown());
                });

                x.RunAsLocalSystem();

                x.StartAutomaticallyDelayed();

                x.EnableServiceRecovery(r => {
                    //
                    r.RestartService(0);

                    r.OnCrashOnly();

                    // Number of days until the error count resets
                    r.SetResetPeriod(1);
                });

                // Specifies that the service supports the shutdown service command.
                x.EnableShutdown();

                x.SetDescription(description);
                x.SetDisplayName(displayName);
                x.SetServiceName(serviceName);
            });
        }

        private static IContainer CreateContainer()
        {
            BasicConfigurator.Configure();
            var logger = LogManager.GetLogger(typeof(Service.WinService));

            ContainerBuilder containerBuilder = new ContainerBuilder();

            Voise.IocModule.BuildContainer(containerBuilder, logger);

            containerBuilder.RegisterType<Service.WinService>()
                .AsSelf()
                .InstancePerLifetimeScope();

            return containerBuilder.Build();
        }
    }
}
