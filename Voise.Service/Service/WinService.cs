using Autofac;
using log4net;
using System;
using System.Threading;
using Voise.Classification.Interface;
using Voise.General;
using Voise.General.Interface;
using Voise.Recognizer.Interface;
using Voise.Synthesizer.Interface;
using Voise.Tuning.Interface;

namespace VoiseService.Service
{
    internal class WinService
    {
        private VoiseServer _voise;

        private ILog _logger;
        private CancellationTokenSource _cancel;

        public WinService(ILog logger)
        {
            _logger = logger;
            _cancel = new CancellationTokenSource();
        }

        public bool Start(IContainer container)
        {
            _logger.Info($"{nameof(WinService)} Start command received.");

            try
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    _voise = new VoiseServer(
                        scope.Resolve<IConfig>(),
                        scope.Resolve<IRecognizerManager>(),
                        scope.Resolve<IClassifierManager>(),
                        scope.Resolve<ISynthesizerManager>(),
                        scope.Resolve<ITuningManager>(),
                        _logger);
                }

                _voise.StartAsync(_cancel.Token).Wait();

                return true;
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                    _logger.Fatal($"{ie.Message}\nStacktrace: {ie.StackTrace}");

                return false;
            }
            catch(Exception e)
            {
                _logger.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
                return false;
            }
        }

        public bool Stop()
        {
            _logger.Debug($"{nameof(WinService)} Stop command received.");

            _voise?.StopAsync(_cancel.Token).Wait();

            return true;
        }

        public bool Shutdown()
        {
            _logger.Debug($"{nameof(WinService)} Shutdown command received.");

            _voise?.StopAsync(_cancel.Token).Wait();

            return true;
        }
    }
}
