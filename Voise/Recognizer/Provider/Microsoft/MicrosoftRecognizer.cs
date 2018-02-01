using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Microsoft.Job;
using Voise.Synthesizer.Microsoft;

namespace Voise.Recognizer.Provider.Microsoft
{
    internal sealed class MicrosoftRecognizer : CommonRecognizer
    {
        internal const string ENGINE_IDENTIFIER = "me";

        private const int TIMEOUT_TASK_REMOVE_JOBS_ABORTED = 5 * 60 * 1000; // 5 minutes

        internal MicrosoftRecognizer()
        {
            InitRemoveJobsWithAbortedStream();
        }

        protected override ISyncJob CreateSyncJob(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
        }

        protected override IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new StreamingJob(streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
        }

        private AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case "flac":
                    throw new System.Exception("Codec 'flac' not supported.");

                case "linear16":
                    return AudioEncoding.Linear16;

                case "alaw":
                    return AudioEncoding.Alaw;

                case "mulaw":
                    return AudioEncoding.Mulaw;

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
