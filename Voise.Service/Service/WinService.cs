using Common.Logging;
using System;
using Topshelf;
using Voise.General;

namespace VoiseService.Service
{
    internal class WinService
    {
        private VoiseServer _voise;

        public ILog Log { get; private set; }

        public WinService(ILog logger)
        {
            // IocModule.cs needs to be updated in case new paramteres are added to this constructor

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            Log = logger;
        }

        public bool Start(HostControl hostControl)
        {
            Log.Info($"{nameof(Service.WinService)} Start command received.");

            try
            {
                Voise.General.Config config = new Voise.General.Config();

                _voise = new VoiseServer(config);
                _voise.Start();

                return true;
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                    Log.Fatal($"{ie.Message}\nStacktrace: {ie.StackTrace}");

                return false;
            }
            catch(Exception e)
            {
                Log.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
                return false;
            }
        }

        public bool Stop(HostControl hostControl)
        {
            Log.Trace($"{nameof(Service.WinService)} Stop command received.");

            _voise?.Stop();

            return true;
        }

        public bool Shutdown(HostControl hostControl)
        {
            Log.Trace($"{nameof(Service.WinService)} Shutdown command received.");

            _voise?.Stop();

            return true;
        }
    }
}
