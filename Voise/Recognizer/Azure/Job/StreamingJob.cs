using log4net;
using Microsoft.CognitiveServices.SpeechRecognition;
using System.Threading;
using Voise.Recognizer.Common.Job;
using Voise.Synthesizer.Azure;
using static Voise.AudioStream;

namespace Voise.Recognizer.Azure.Job
{
    internal class StreamingJob : Base, IStreamingJob
    {
        private AudioStream _streamIn;

        internal StreamingJob(string primaryKey, AudioStream streamIn, AudioEncoding encoding, int sampleRate, string languageCode)
            : base(LogManager.GetLogger(typeof(StreamingJob)))
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _recognitionClient = SpeechRecognitionServiceFactory.CreateDataClient(
                SpeechRecognitionMode.ShortPhrase, // Áudio de até 15 segundos
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

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
        }

        public void Start()
        {
            _streamIn.Start();
        }

        public void Stop()
        {
            _streamIn.Stop();

            _recognitionClient.EndAudio();

            lock (_monitorCompleted)
            {
                if (!_completed)
                    Monitor.Wait(_monitorCompleted);
            }
        }

        private void ConsumeStreamData(object sender, StreamInEventArgs e)
        {
            _recognitionClient.SendAudio(e.Buffer, e.BytesStreamed);
        }
    }
}
