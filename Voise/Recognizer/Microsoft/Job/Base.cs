using Google.Cloud.Speech.V1Beta1;
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

        protected readonly object _completed;

        public SpeechRecognitionAlternative BestAlternative { get; protected set; }

        protected Base()
        {
            _completed = new object();

            // Set as default alterative
            BestAlternative = NoResultSpeechRecognitionAlternative.Default;
        }

        protected void RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine("RecognizeCompleted, error occurred during recognition: {0}", e.Error);
            }
            else if (e.InitialSilenceTimeout || e.BabbleTimeout)
            {
                // This not must be raised!
                // TODO: Log this!
            }
            else if (e.Result != null)
            {
                BestAlternative = new SpeechRecognitionAlternative();
                BestAlternative.Transcript = e.Result.Text;
                BestAlternative.Confidence = e.Result.Confidence;
            }

            lock (_completed)
                Monitor.Pulse(_completed);
        }

        protected void ValidateArguments(AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");

            if (contexts == null || contexts.Count == 0)
                throw new System.Exception("Context is not defined.");
        }

        protected byte[] ConvertAudioToBytes(string audio_base64)
        {
            if (audio_base64 == null || audio_base64.Trim() == string.Empty)
                throw new BadAudioException("Audio is empty.");

            try
            {
                return Convert.FromBase64String(audio_base64);
            }
            catch (System.Exception e)
            {
                throw new BadAudioException("Audio is invalid.", e);
            }
        }
    }
}
