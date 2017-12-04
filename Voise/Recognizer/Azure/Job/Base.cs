﻿using Google.Cloud.Speech.V1Beta1;
using log4net;
using Microsoft.CognitiveServices.SpeechRecognition;
using System.Linq;
using System.Threading;
using Voise.Recognizer.Exception;
using Voise.Synthesizer.Azure;

namespace Voise.Recognizer.Azure.Job
{
    internal abstract class Base
    {
        protected DataRecognitionClient _recognitionClient;

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

        protected void ValidateArguments(AudioEncoding encoding, int sampleRate, string languageCode)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");
        }

        protected void ResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.RecognitionSuccess)
            {
                RecognizedPhrase bestResult = e.PhraseResponse.Results.OrderByDescending(x => (int)x.Confidence).First();

                BestAlternative = new SpeechRecognitionAlternative();
                BestAlternative.Transcript = bestResult.DisplayText;
                BestAlternative.Confidence = ConvertConfidence(bestResult.Confidence);
            }

            lock (_monitorCompleted)
            {
                _completed = true;
                Monitor.Pulse(_monitorCompleted);
            }
        }

        protected void ConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            _log.Error($"{e.SpeechErrorText} Code: {e.SpeechErrorCode.ToString()}");

            lock (_monitorCompleted)
            {
                _completed = true;
                Monitor.Pulse(_monitorCompleted);
            }
        }

        protected float ConvertConfidence(Confidence confidence)
        {
            switch (confidence)
            {
                case Confidence.Low:
                    return 0.10F;
                case Confidence.Normal:
                    return 0.50F;
                case Confidence.High:
                    return 0.90F;
                default:
                    return 0;
            }
        }
    }
}