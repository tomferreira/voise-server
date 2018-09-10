using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Common
{
    internal abstract class CommonRecognizer
    {
        private const int TIMEOUT_TASK_REMOVE_JOBS_ABORTED = 5 * 60 * 1000; // 5 minutes

        protected Dictionary<AudioStream, IStreamingJob> _streamingJobs;

        internal CommonRecognizer()
        {
            _streamingJobs = new Dictionary<AudioStream, IStreamingJob>();

            InitRemoveJobsWithAbortedStream();
        }

        internal async Task<SpeechRecognitionResult> SyncRecognition(byte[] audio, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            using (ISyncJob job = CreateSyncJob(audio, encoding, sampleRate, languageCode, contexts))
            {
                await job.StartAsync().ConfigureAwait(false);

                return job.BestAlternative;
            }
        }

        internal async Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            IStreamingJob job = CreateStreamingJob(streamIn, encoding, sampleRate, languageCode, contexts);

            lock (_streamingJobs)
                _streamingJobs.Add(streamIn, job);

            await job.StartAsync().ConfigureAwait(false);
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

            return result;
        }

        protected abstract ISyncJob CreateSyncJob(byte[] audio, string encoding,
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
