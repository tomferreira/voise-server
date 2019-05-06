using log4net;
using log4net.Config;
using System;
using System.Threading;
using Voise.General;

namespace Voise
{
    public class Entry
    {
        public static void Main()
        {
#if DEBUG
            BasicConfigurator.Configure();

            try
            {
                Config config = new Config();

                VoiseServer voise = new VoiseServer(config);
                voise.Start();

                while (true)
                    Thread.Sleep(100);
            }
            catch (Exception e)
            {
                ILog log = LogManager.GetLogger(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

                if (e is AggregateException)
                {
                    foreach (var ie in (e as AggregateException).InnerExceptions)
                        log.Fatal($"{ie.Message}\nStacktrace: {ie.StackTrace}");
                }
                else
                {
                    log.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
                }
            }
#else
            Console.WriteLine("To start Voise Server, use the Windows Service.");
#endif
        }
    }
}
