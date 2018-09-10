using log4net;
using System;
using System.IO;
using System.Threading;
using Voise.General;
using Voise.TCP.Request;

namespace Voise.Tuning
{
    internal class TuningManager
    {
        private string _tuningPath;
        private int _retentionDays;

        private ILog _log;

        private Thread _retentionPolice;

        internal TuningManager(Config config)
        {
            _log = LogManager.GetLogger(typeof(TuningManager));

            _tuningPath = null;
            _retentionDays = 0;

            if (config.TuningEnabled)
                EnableTuning(config.TuningPath, config.TuningRetentionDays);
        }

        internal TuningIn CreateTuningIn(Base.InputMethod inputMethod, VoiseConfig config)
        {
            if (string.IsNullOrWhiteSpace(_tuningPath))
                return null;

            return new TuningIn(_tuningPath, inputMethod, config);
        }

        internal TuningOut CreateTuningOut(Base.InputMethod inputMethod, string text, VoiseConfig config)
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

                    DateTime directoryDate;
                    if (DateTime.TryParse(directoryName, out directoryDate))
                    {
                        if ((now - directoryDate).Days >= _retentionDays)
                        {
                            try
                            {
                                Directory.Delete(directoryPath, true);

                                _log.Info($"Directory '{directoryPath}' excluded successful.");
                            }
                            catch
                            {
                                _log.Error($"Can not delete the directory '{directoryPath}'.");
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _log.Error($"{ex.Message}");
            }
        }
    }
}
