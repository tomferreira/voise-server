using Autofac;
using Autofac.Extensions.DependencyInjection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Voise.General;

namespace Voise
{
    public static class Entry
    {
        private static ILog _logger;

        static async Task Main()
        {
#if DEBUG
            BasicConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(Entry));

            try
            {
                await Host.CreateDefaultBuilder()
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureServices(ConfigureServices)
                    .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
                    .RunConsoleAsync();
            }
            catch (Exception e)
            {
                IocModule.LogDeepestExceptions(e, _logger);
            }
#else
            await Console.Out.WriteLineAsync("To start Voise Server, use the Windows Service.");
#endif
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Use extensions from libraries to register services in the
            // collection. These will be automatically added to the Autofac
            // container.

            services.AddHostedService<VoiseServer>();
        }

        private static void ConfigureContainer(ContainerBuilder containerBuilder)
        {
            // Add any Autofac modules or registrations. This is called AFTER
            // ConfigureServices so things you register here OVERRIDE things
            // registered in ConfigureServices.

            IocModule.BuildContainer(containerBuilder, _logger);
        }
    }
}
