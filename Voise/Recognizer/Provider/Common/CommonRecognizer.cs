
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Common
{
    internal abstract class CommonRecognizer
    {
        protected Dictionary<AudioStream, IStreamingJob> _streamingJobs;

        internal CommonRecognizer()
        {
            _streamingJobs = new Dictionary<AudioStream, IStreamingJob>();
        }

        internal async Task<SpeechRecognitionResult> SyncRecognition(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            using (ISyncJob job = CreateSyncJob(audio_base64, encoding, sampleRate, languageCode, contexts))
            {
                await job.StartAsync();

                return job.BestAlternative;
            }
        }

        internal async Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            IStreamingJob job = CreateStreamingJob(streamIn, encoding, sampleRate, languageCode, contexts);

            lock (_streamingJobs)
                _streamingJobs.Add(streamIn, job);

            await job.StartAsync();
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

            return result;
        }

        protected abstract ISyncJob CreateSyncJob(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);

        protected abstract IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);
    }
}
