using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Recognizer.Microsoft.Job;
using Voise.Synthesizer.Microsoft;

namespace Voise.Recognizer.Microsoft
{
    internal sealed class MicrosoftRecognizer : Base
    {
        internal const string ENGINE_IDENTIFIER = "me";

        private Dictionary<AudioStream, StreamingJob> _streamingJobs;

        internal MicrosoftRecognizer()
        {
            _streamingJobs = new Dictionary<AudioStream, StreamingJob>();
        }

        internal override async Task<SpeechRecognitionAlternative> SyncRecognition(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            SyncJob job = new SyncJob(audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);

            job.Start();

            return job.BestAlternative;
        }

        internal override async Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            StreamingJob job = new StreamingJob(streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
            _streamingJobs.Add(streamIn, job);

            job.Start();
        }

        internal override async Task<SpeechRecognitionAlternative> StopStreamingRecognitionAsync(AudioStream streamIn)
        {
            if (!_streamingJobs.ContainsKey(streamIn))
                throw new System.Exception("Job not exists.");

            StreamingJob job = _streamingJobs[streamIn];

            job.Stop();

            _streamingJobs.Remove(streamIn);

            return job.BestAlternative;
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
    }
}
