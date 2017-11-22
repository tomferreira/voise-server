using Google.Cloud.Speech.V1Beta1;
using log4net;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Threading;
using Voise.Recognizer.Exception;
using Voise.Synthesizer.Microsoft;

namespace Voise.Recognizer.Microsoft.Job
{
    internal abstract class Base
    {
        protected SpeechRecognitionEngine _engine;
        protected SpeechAudioFormatInfo _info;

        protected bool _completed;
        protected readonly object _monitorCompleted;

        protected ILog _log;

        public SpeechRecognitionAlternative BestAlternative { get; protected set; }

        protected Base()
        {
            _monitorCompleted = new object();
            _completed = false;

            // Set as default alterative
            BestAlternative = NoResultSpeechRecognitionAlternative.Default;
        }

        protected void RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _log.Error($"Error occurred during recognition: {e.Error}");
            }
            else if (e.InitialSilenceTimeout || e.BabbleTimeout)
            {
                _log.Error("Initial silence timeout ou babble timeout raised");
            }
            else if (e.Result != null)
            {
                BestAlternative = new SpeechRecognitionAlternative();
                BestAlternative.Transcript = e.Result.Text;
                BestAlternative.Confidence = e.Result.Confidence;
            }

            lock (_monitorCompleted)
            {
                _completed = true;
                Monitor.Pulse(_monitorCompleted);
            }
        }

        protected void ValidateArguments(AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");

            if (contexts == null || contexts.Count == 0)
                throw new System.Exception("Context is not defined. (This is only required to Microsoft engine, for all others this is optional)");
        }
    }
}
