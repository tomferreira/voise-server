using log4net;
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

            InitClient(primaryKey, encoding, sampleRate, languageCode);

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
        }

        public async Task StartAsync()
        {
            await Task.Run(() => _streamIn.Start()).ConfigureAwait(false);
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
            }).ConfigureAwait(false);
        }

        private void ConsumeStreamData(object sender, StreamInEventArgs e)
        {
            _recognitionClient.SendAudio(e.Buffer, e.BytesStreamed);
        }
    }
}
