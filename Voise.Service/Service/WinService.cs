﻿using Common.Logging;
using System;
using Topshelf;

namespace VoiseService.Service
{
    class WinService
    {
        private Voise.Voise _voise;

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
                Voise.Config config = new Voise.Config();

                _voise = new Voise.Voise(config);
                _voise.Start();

                return true;
            }
            catch (Exception e)
            {
                if (e is AggregateException)
                {
                    foreach (var ie in (e as AggregateException).InnerExceptions)
                        Log.Fatal($"{ie.Message}\nStacktrace: {ie.StackTrace}");
                }
                else
                {
                    Log.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
                }

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
