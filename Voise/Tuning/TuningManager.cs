using Voise.TCP.Request;

namespace Voise.Tuning
{
    internal class TuningManager
    {
        private string _tuningPath;

        internal TuningManager(Config config)
        {
            _tuningPath = null;

            if (config.TuningEnabled)
                EnableTuning(config.TuningPath);
        }

        internal TuningIn CreateTuningIn(Base.InputMethod inputMethod, VoiseConfig config)
        {
            if (string.IsNullOrWhiteSpace(_tuningPath))
                return null;

            return new TuningIn(_tuningPath, inputMethod, config);
        }

        private void EnableTuning(string tuningPath)
        {
            _tuningPath = tuningPath;
        }
    }
}
