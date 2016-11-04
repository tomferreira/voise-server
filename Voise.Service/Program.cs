using System;
using System.IO;
using Topshelf;
using Topshelf.Common.Logging;
using Topshelf.Ninject;

namespace VoiseService
{
    class Program
    {
        static void Main(string[] args)
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
                x.UseCommonLogging();
                x.UseNinject(new IocModule());

                x.Service<Service.WinService>(sc =>
                {
                    sc.ConstructUsingNinject();

                    // the start and stop methods for the service
                    sc.WhenStarted((s, hostControl) => s.Start(hostControl));
                    sc.WhenStopped((s, hostControl) => s.Stop(hostControl));

                    // optional pause/continue methods if used
                    sc.WhenPaused((s, hostControl) => s.Pause(hostControl));
                    sc.WhenContinued((s, hostControl) => s.Continue(hostControl));

                    // optional, when shutdown is supported
                    sc.WhenShutdown((s, hostControl) => s.Shutdown(hostControl));

                });

                //=> Service Identity

                x.RunAsLocalSystem();

                //x.RunAs("username", "password"); // predefined user
                //x.RunAsPrompt(); // when service is installed, the installer will prompt for a username and password
                //x.RunAsNetworkService(); // runs as the NETWORK_SERVICE built-in account
                //x.RunAsLocalSystem(); // run as the local system account
                //x.RunAsLocalService(); // run as the local service account

                x.StartAutomaticallyDelayed();

                //=> Service Configuration

                //x.EnablePauseAndContinue(); // Specifies that the service supports pause and continue.
                //x.EnableShutdown(); //Specifies that the service supports the shutdown service command.

                x.SetDescription(description);
                x.SetDisplayName(displayName);
                x.SetServiceName(serviceName);
            });
        }
    }
}
