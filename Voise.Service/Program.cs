﻿using System;
using System.IO;
using Topshelf;
using Topshelf.Common.Logging;
using Topshelf.Ninject;

namespace VoiseService
{
    class Program
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
                x.UseCommonLogging();
                x.UseNinject(new IocModule());

                x.Service<Service.WinService>(sc =>
                {
                    sc.ConstructUsingNinject();

                    // the start and stop methods for the service
                    sc.WhenStarted((s, hostControl) => s.Start(hostControl));
                    sc.WhenStopped((s, hostControl) => s.Stop(hostControl));

                    // optional, when shutdown is supported
                    sc.WhenShutdown((s, hostControl) => s.Shutdown(hostControl));

                });

                x.RunAsLocalSystem();

                x.StartAutomaticallyDelayed();

                // Specifies that the service supports the shutdown service command.
                x.EnableShutdown();

                x.SetDescription(description);
                x.SetDisplayName(displayName);
                x.SetServiceName(serviceName);
            });
        }
    }
}
