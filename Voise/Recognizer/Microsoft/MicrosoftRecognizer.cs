using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voise.Recognizer.Microsoft.Job;
using Voise.Synthesizer.Microsoft;

namespace Voise.Recognizer.Microsoft
{
    internal sealed class MicrosoftRecognizer : Base
    {
        internal const string ENGINE_IDENTIFIER = "me";

        private const int TIMEOUT_TASK_REMOVE_JOBS_ABORTED = 5 * 60 * 1000; // 5 minutes

        private Dictionary<AudioStream, StreamingJob> _streamingJobs;

        internal MicrosoftRecognizer()
        {
            _streamingJobs = new Dictionary<AudioStream, StreamingJob>();

            InitRemoveJobsWithAbortedStream();
        }

        internal override async Task<SpeechRecognitionAlternative> SyncRecognition(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            using (SyncJob job = new SyncJob(audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts))
            {
                job.Start();

                return job.BestAlternative;
            }
        }

        internal override async Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            StreamingJob job = new StreamingJob(streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);

            lock (_streamingJobs)
                _streamingJobs.Add(streamIn, job);

            job.Start();
        }

        internal override async Task<SpeechRecognitionAlternative> StopStreamingRecognitionAsync(AudioStream streamIn)
        {
            StreamingJob job = null;

            lock (_streamingJobs)
            {
                if (!_streamingJobs.ContainsKey(streamIn))
                    throw new System.Exception("Job not exists.");

                job = _streamingJobs[streamIn];
            }

            job.Stop();

            SpeechRecognitionAlternative result = job.BestAlternative;

            lock (_streamingJobs)
                _streamingJobs.Remove(streamIn);

            job.Dispose();

            return result;
        }

        private AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case "alaw":
                    return AudioEncoding.Alaw;

                case "mulaw":
                    return AudioEncoding.Mulaw;

                case "linear16":
                    return AudioEncoding.Linear16;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }

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
                            await StopStreamingRecognitionAsync(stream);
                    }
                    catch
                    {
                        // Ignore any exception
                    }
                    finally
                    {
                        await Task.Delay(TIMEOUT_TASK_REMOVE_JOBS_ABORTED);
                    }
                }
            });
        }
    }
}
