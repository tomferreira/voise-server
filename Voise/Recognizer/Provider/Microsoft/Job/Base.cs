using log4net;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Voise.Provider.Microsoft;
using Voise.Recognizer.Exception;

namespace Voise.Recognizer.Provider.Microsoft.Job
{
    internal abstract class Base : IDisposable
    {
        protected SpeechRecognitionEngine _engine;

        protected bool _completed;
        protected readonly object _monitorCompleted;

        protected ILog _log;

        protected bool _disposed;

        public SpeechRecognitionResult BestAlternative { get; protected set; }

        protected Base(ILog log)
        {
            _log = log;

            _monitorCompleted = new object();
            _completed = false;

            _disposed = false;

            // Set as default alternative
            BestAlternative = SpeechRecognitionResult.NoResult;
        }

        protected void RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _log.Error($"Error occurred during recognition: {e.Error}");
            }
            else if (e.InitialSilenceTimeout || e.BabbleTimeout)
            {
                _log.Info("Initial silence timeout ou babble timeout raised.");
            }
            else if (e.Result != null)
            {
                BestAlternative = new SpeechRecognitionResult(
                    e.Result.Text, e.Result.Confidence);
            }

            lock (_monitorCompleted)
            {
                _completed = true;
                Monitor.Pulse(_monitorCompleted);
            }
        }

        protected static void ValidateArguments(AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");

            if (contexts == null || contexts.Count == 0)
                throw new System.Exception("Context is not defined. (This is only required to Microsoft engine, for all others this is optional)");
        }

        protected RecognizerInfo GetInstalledRecognizerInfo(string languageCode, bool strict)
        {
            if (languageCode == null)
                return null;

            var installedInfos = SpeechRecognitionEngine.InstalledRecognizers();

            var infoSelected = installedInfos.FirstOrDefault(
                    info => string.Equals(info.Culture.Name, languageCode, StringComparison.OrdinalIgnoreCase));

            if (strict || infoSelected != null)
                return infoSelected;

            string prefixLanguageCode = languageCode.Split('-').First();

            return installedInfos.FirstOrDefault(
                info => string.Equals(info.Culture.Name.Split('-').First(), prefixLanguageCode, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _engine.Dispose();

            _disposed = true;
        }
    }
}
