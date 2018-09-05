using CPqDAsr.ASR;
using CPqDASR.Config;
using log4net;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Tuning;

namespace Voise.Recognizer.Provider.Cpqd.Job
{
    internal class SyncJob : Base, ISyncJob
    {
        public SyncJob(ClientConfig config, LanguageModelList modelList, string audio_base64, AudioEncoding encoding, int sampleRate, string languageCode)
            : base(LogManager.GetLogger(typeof(SyncJob)), config, modelList)
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            var audioBytes = Util.ConvertAudioToBytes(audio_base64);
            _audioSource = new Internal.BufferAudioSource(audioBytes);
        }

        public async Task StartAsync(TuningIn tuning)
        {
            _tuning = tuning;

            var audio = _audioSource.Read();
            _tuning?.WriteRecording(audio, 0, audio.Length);

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