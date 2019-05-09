using log4net;
using log4net.Config;
using System;
using System.Threading;
using Voise.General;

namespace Voise
{
    public static class Entry
    {
        public static void Main()
        {
#if DEBUG
            BasicConfigurator.Configure();

            ILog log = LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            try
            {
                Config config = new Config();

                VoiseServer voise = new VoiseServer(config);
                voise.Start();

                while (true)
                    Thread.Sleep(100);
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                    log.Fatal($"{ie.Message}\nStacktrace: {ie.StackTrace}");
            }
            catch (Exception e)
            {
                log.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
            }
#else
            Console.WriteLine("To start Voise Server, use the Windows Service.");
#endif
        }
    }
}
