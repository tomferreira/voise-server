using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Google.Cloud.Speech.V1Beta1;
using Voise.Recognizer.Google.Job;
using static Google.Cloud.Speech.V1Beta1.RecognitionConfig.Types;

namespace Voise.Recognizer.Google
{
    internal sealed class GoogleRecognizer
    {
        private SpeechRecognizer _recognizer;
        private Dictionary<AudioStream, StreamingJob> _streamingJobs;
        private string _tunningPath;

        internal GoogleRecognizer()
        {
            _recognizer = SpeechRecognizer.Create();
            _streamingJobs = new Dictionary<AudioStream, StreamingJob>();
            _tunningPath = null;
        }

        internal void EnableTunnig(string path)
        {
            _tunningPath = path;
        }

        // Max duration of audio ~60s (https://cloud.google.com/speech/limits)
        internal async Task<SpeechRecognitionAlternative> SyncRecognition(string audio_base64, AudioEncoding encoding, 
            int sampleRate, string languageCode, List<string> context)
        {
            SyncJob job = new SyncJob(audio_base64, encoding, sampleRate, languageCode, context);

            await job.StartAsync(_recognizer);

            return job.BestAlternative;
        }

        internal async Task StartStreamingRecognitionAsync(AudioStream streamIn, AudioEncoding encoding, 
            int sampleRate, string languageCode, List<string> context)
        {
            StreamingJob job = new StreamingJob(streamIn, encoding, sampleRate, languageCode, context);
            _streamingJobs.Add(streamIn, job);

            await job.StartAsync(_recognizer);
        }

        internal async Task<SpeechRecognitionAlternative> StopStreamingRecognitionAsync(AudioStream streamIn)
        {
            if (!_streamingJobs.ContainsKey(streamIn))
                throw new System.Exception("Job not exists.");

            StreamingJob job = _streamingJobs[streamIn];

            await job.Stop();

            _streamingJobs.Remove(streamIn);

            return job.BestAlternative;
        }

        internal static AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case "flac":
                    return AudioEncoding.Flac;

                case "linear16":
                    return AudioEncoding.Linear16;

                case "mulaw":
                    return AudioEncoding.Mulaw;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}
