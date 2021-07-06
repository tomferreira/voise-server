using log4net;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Azure.Job
{
    internal class SyncJob : Base, ISyncJob
    {
        private readonly byte[] _audio;

        internal SyncJob(string primaryKey, byte[] audio, AudioEncoding encoding, int sampleRate, string languageCode)
            : base(LogManager.GetLogger(typeof(SyncJob)))
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            InitClient(primaryKey, encoding, sampleRate, languageCode);

            _audio = audio;
        }

        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                _recognitionClient.SendAudio(_audio, _audio.Length);
                _recognitionClient.EndAudio();

                lock (_monitorCompleted)
                {
                    if (!_completed)
                        Monitor.Wait(_monitorCompleted);
                }
            }).ConfigureAwait(false);
        }
    }
}
