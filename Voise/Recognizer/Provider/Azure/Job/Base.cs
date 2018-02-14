using log4net;
using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Exception;

namespace Voise.Recognizer.Provider.Azure.Job
{
    internal abstract class Base: IDisposable
    {
        protected DataRecognitionClient _recognitionClient;

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

        protected void ValidateArguments(AudioEncoding encoding, int sampleRate, string languageCode)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");
        }

        protected void InitClient(string primaryKey, AudioEncoding encoding, int sampleRate, string languageCode)
        {
            _recognitionClient = SpeechRecognitionServiceFactory.CreateDataClient(
                SpeechRecognitionMode.ShortPhrase, // Audio up to 15 seconds
                languageCode,
                primaryKey);

            _recognitionClient.OnResponseReceived += ResponseReceivedHandler;
            _recognitionClient.OnConversationError += ConversationErrorHandler;

            SpeechAudioFormat format = new SpeechAudioFormat()
            {
                EncodingFormat = encoding.Format,
                SamplesPerSecond = sampleRate,
                BitsPerSample = encoding.BitsPerSample,
                ChannelCount = encoding.ChannelCount,
                AverageBytesPerSecond = sampleRate * encoding.BitsPerSample / 8,
                BlockAlign = encoding.BlockAlign
            };

            _recognitionClient.SendAudioFormat(format);
        }

        protected void ResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.RecognitionSuccess)
            {
                RecognizedPhrase bestResult = e.PhraseResponse.Results.OrderByDescending(x => (int)x.Confidence).First();

                BestAlternative = new SpeechRecognitionResult(
                    bestResult.DisplayText, ConvertConfidence(bestResult.Confidence));
            }

            lock (_monitorCompleted)
            {
                _completed = true;
                Monitor.Pulse(_monitorCompleted);
            }
        }

        protected void ConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            _log.Error($"{e.SpeechErrorText} [Code: {e.SpeechErrorCode.ToString()}]");

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
            {
                // FIXME: This isn't the best approach, but the Dispose method 
                // is take 2 sec, and its very slow.
                // _recognitionClient.Dispose();
            }

            _disposed = true;
        }
    }
}
