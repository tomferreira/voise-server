using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Tuning;

namespace Voise.Recognizer.Provider.Common
{
    internal abstract class CommonRecognizer
    {
        private const int TIMEOUT_TASK_REMOVE_JOBS_ABORTED = 5 * 60 * 1000; // 5 minutes

        private string _tuningPath;
        protected Dictionary<AudioStream, IStreamingJob> _streamingJobs;

        internal CommonRecognizer()
        {
            _streamingJobs = new Dictionary<AudioStream, IStreamingJob>();

            InitRemoveJobsWithAbortedStream();
        }

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

                await job.StartAsync(tuning).ConfigureAwait(false);

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

            await job.StartAsync(tuning).ConfigureAwait(false);
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

            await job.StopAsync().ConfigureAwait(false);

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

        // This task runs in background mode and its responsible to stop the aborted audio streaming.
        // The abort can occur when the socket connection is finished during the process of send audio streaming.
        private void InitRemoveJobsWithAbortedStream()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        IEnumerable<AudioStream> abortedStreams = null;

                        lock (_streamingJobs)
                            abortedStreams = _streamingJobs.Keys.Where(s => s.IsAborted());

                        foreach (AudioStream stream in abortedStreams)
                            await StopStreamingRecognitionAsync(stream).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Ignore any exception
                    }
                    finally
                    {
                        await Task.Delay(TIMEOUT_TASK_REMOVE_JOBS_ABORTED).ConfigureAwait(false);
                    }
                }
            });
        }
    }
}
