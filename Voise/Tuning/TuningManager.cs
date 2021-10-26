using log4net;
using System;
using System.IO;
using System.Threading;
using Voise.General.Interface;
using Voise.TCP.Request;
using Voise.Tuning.Interface;

namespace Voise.Tuning
{
    public class TuningManager : ITuningManager
    {
        private string _tuningPath;
        private int _retentionDays;

        private readonly ILog _logger;

        private Thread _retentionPolice;

        public TuningManager(IConfig config, ILog logger)
        {
            _logger = logger;

            _tuningPath = null;
            _retentionDays = 0;

            if (config.TuningEnabled)
                EnableTuning(config.TuningPath, config.TuningRetentionDays);
        }

        public TuningIn CreateTuningIn(Base.InputMethod inputMethod, VoiseConfig config)
        {
            if (string.IsNullOrWhiteSpace(_tuningPath))
                return null;

            return new TuningIn(_tuningPath, inputMethod, config);
        }

        public TuningOut CreateTuningOut(Base.InputMethod inputMethod, string text, VoiseConfig config)
        {
            if (string.IsNullOrWhiteSpace(_tuningPath))
                return null;

            return new TuningOut(_tuningPath, inputMethod, text, config);
        }

        private void EnableTuning(string tuningPath, int retentionDays)
        {
            _tuningPath = tuningPath;
            _retentionDays = retentionDays;

            _retentionPolice = new Thread(() =>
            {
                while (true)
                {
                    DeleteOldTuning();

                    Thread.Sleep(TimeSpan.FromHours(1));
                }
            });

            _retentionPolice.Start();
        }

        private void DeleteOldTuning()
        {
            DateTime now = DateTime.Now;

            try
            {
                foreach (var directoryPath in Directory.GetDirectories(_tuningPath, "*", SearchOption.TopDirectoryOnly))
                {
                    var directoryName = Path.GetFileName(Path.GetDirectoryName(directoryPath + Path.DirectorySeparatorChar));

                    if (DateTime.TryParse(directoryName, out DateTime directoryDate))
                    {
                        if ((now - directoryDate).Days >= _retentionDays)
                        {
                            try
                            {
                                Directory.Delete(directoryPath, true);

                                _logger.Info($"Directory '{directoryPath}' excluded successful.");
                            }
                            catch
                            {
                                _logger.Error($"Can not delete the directory '{directoryPath}'.");
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Error($"{ex.Message}");
            }
        }
    }
}
