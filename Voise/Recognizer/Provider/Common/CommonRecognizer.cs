
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Tuning;

namespace Voise.Recognizer.Provider.Common
{
    internal abstract class CommonRecognizer
    {
        private string _tuningPath;
        private Dictionary<AudioStream, IStreamingJob> _streamingJobs = new Dictionary<AudioStream, IStreamingJob>();

        internal void EnableTuning(string tuningPath)
        {
            _tuningPath = tuningPath;
        }

        internal async Task<SpeechRecognitionResult> SyncRecognition(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            using (ISyncJob job = CreateSyncJob(audio_base64, encoding, sampleRate, languageCode, contexts))
            {
                TuningIn tuning = _tuningPath != null ? 
                    new TuningIn(_tuningPath, TuningIn.InputMethod.Sync, encoding, sampleRate, languageCode) : null;

                await job.StartAsync(tuning);

                tuning?.Dispose();

                return job.BestAlternative;
            }
        }

        internal async Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            IStreamingJob job = CreateStreamingJob(streamIn, encoding, sampleRate, languageCode, contexts);

            lock (_streamingJobs)
                _streamingJobs.Add(streamIn, job);

            TuningIn tuning = _tuningPath != null ?
                new TuningIn(_tuningPath, TuningIn.InputMethod.Stream, encoding, sampleRate, languageCode) : null;

            await job.StartAsync(tuning);
        }

        internal async Task<SpeechRecognitionResult> StopStreamingRecognitionAsync(AudioStream streamIn)
        {
            IStreamingJob job = null;

            lock (_streamingJobs)
            {
                if (!_streamingJobs.ContainsKey(streamIn))
                    throw new System.Exception("Job not exists.");

                job = _streamingJobs[streamIn];
            }

            await job.StopAsync();

            SpeechRecognitionResult result = job.BestAlternative;

            lock (_streamingJobs)
                _streamingJobs.Remove(streamIn);

            job.Dispose();

            // TODO: Dispose tuning

            return result;
        }

        protected abstract ISyncJob CreateSyncJob(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);

        protected abstract IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);
    }
}
