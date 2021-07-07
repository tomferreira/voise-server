using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voise.General.Interface;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Common
{
    internal abstract class CommonRecognizer : ICommonRecognizer
    {
        private const int TIMEOUT_TASK_REMOVE_JOBS_ABORTED = 5 * 60 * 1000; // 5 minutes

        protected Dictionary<IAudioStream, IStreamingJob> _streamingJobs;

        protected CommonRecognizer()
        {
            _streamingJobs = new Dictionary<IAudioStream, IStreamingJob>();

            InitRemoveJobsWithAbortedStream();
        }

        public async Task<SpeechRecognitionResult> SyncRecognition(byte[] audio, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            using (ISyncJob job = CreateSyncJob(audio, encoding, sampleRate, languageCode, contexts))
            {
                await job.StartAsync().ConfigureAwait(false);

                return job.BestAlternative;
            }
        }

        public async Task StartStreamingRecognitionAsync(IAudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            IStreamingJob job = CreateStreamingJob(streamIn, encoding, sampleRate, languageCode, contexts);

            lock (_streamingJobs)
                _streamingJobs.Add(streamIn, job);

            await job.StartAsync().ConfigureAwait(false);
        }

        public async Task<SpeechRecognitionResult> StopStreamingRecognitionAsync(IAudioStream streamIn)
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

        protected abstract IStreamingJob CreateStreamingJob(IAudioStream streamIn, string encoding,
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
                        IEnumerable<IAudioStream> abortedStreams = null;

                        lock (_streamingJobs)
                            abortedStreams = _streamingJobs.Keys.Where(s => s.IsAborted());

                        foreach (IAudioStream stream in abortedStreams)
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
