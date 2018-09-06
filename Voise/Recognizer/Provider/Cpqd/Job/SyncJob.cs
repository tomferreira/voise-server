using CPqDAsr.ASR;
using CPqDASR.Config;
using log4net;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Cpqd.Job
{
    internal class SyncJob : Base, ISyncJob
    {
        public SyncJob(ClientConfig config, LanguageModelList modelList, byte[] audio, AudioEncoding encoding, int sampleRate, string languageCode)
            : base(LogManager.GetLogger(typeof(SyncJob)), config, modelList)
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _audioSource = new Internal.BufferAudioSource(audio);
        }

        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                _speechRecognizer.Recognize(_audioSource, _modelList);

                lock (_monitorCompleted)
                {
                    if (!_completed)
                        Monitor.Wait(_monitorCompleted);
                }
            });
        }
    }
}