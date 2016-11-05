using Google.Cloud.Speech.V1Beta1;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Threading;
using Voise.Recognizer.Exception;

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

        protected void SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            // No result

            lock (_completed)
                Monitor.Pulse(_completed);
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
