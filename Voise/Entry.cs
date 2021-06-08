using Autofac;
using Autofac.Extensions.DependencyInjection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Voise.General;
using Voise.General.Interface;

namespace Voise
{
    public static class Entry
    {
        private static ILog _logger;
        private static IConfig _config;

        static async Task Main()
        {
#if DEBUG
            BasicConfigurator.Configure();
            _logger = LogManager.GetLogger(typeof(Entry));

            _config = new Config();

            try
            {
                await Host.CreateDefaultBuilder()
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureServices(ConfigureServices)
                    .ConfigureContainer<ContainerBuilder>(ConfigureContainer)
                    .RunConsoleAsync();
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                    _logger.Fatal($"{ie.Message}\nStacktrace: {ie.StackTrace}");
            }
            catch (Exception e)
            {
                _logger.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
            }
#else
            Console.WriteLine("To start Voise Server, use the Windows Service.");
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

            IocModule.BuildContainer(containerBuilder, _config, _logger);
        }
    }
}
