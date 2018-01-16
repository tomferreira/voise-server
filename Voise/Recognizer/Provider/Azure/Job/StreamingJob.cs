using log4net;
using Microsoft.CognitiveServices.SpeechRecognition;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;
using static Voise.AudioStream;

namespace Voise.Recognizer.Provider.Azure.Job
{
    internal class StreamingJob : Base, IStreamingJob
    {
        private AudioStream _streamIn;

        internal StreamingJob(string primaryKey, AudioStream streamIn, AudioEncoding encoding, int sampleRate, string languageCode)
            : base(LogManager.GetLogger(typeof(StreamingJob)))
        {
            ValidateArguments(encoding, sampleRate, languageCode);

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

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
        }

        public async Task StartAsync()
        {
            await Task.Run(() => _streamIn.Start() );
        }

        public async Task StopAsync()
        {
            await Task.Run(() =>
            {
                _streamIn.Stop();

                _recognitionClient.EndAudio();

                lock (_monitorCompleted)
                {
                    if (!_completed)
                        Monitor.Wait(_monitorCompleted);
                }
            });
        }

        private void ConsumeStreamData(object sender, StreamInEventArgs e)
        {
            _recognitionClient.SendAudio(e.Buffer, e.BytesStreamed);
        }
    }
}
