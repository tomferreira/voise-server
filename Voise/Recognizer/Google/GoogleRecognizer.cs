using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Google.Cloud.Speech.V1Beta1;
using Voise.Recognizer.Google.Job;
using static Google.Cloud.Speech.V1Beta1.RecognitionConfig.Types;

namespace Voise.Recognizer.Google
{
    internal sealed class GoogleRecognizer : Base
    {
        internal const string ENGINE_IDENTIFIER = "ge";

        private SpeechRecognizer _recognizer;
        private Dictionary<AudioStream, StreamingJob> _streamingJobs;
        private string _tunningPath;

        internal GoogleRecognizer(string credentialPath)
        {
            if (credentialPath == null)
                throw new System.Exception("Credential path must be defined for Google engine.");

            _recognizer = SpeechRecognizer.Create(credentialPath);
            _streamingJobs = new Dictionary<AudioStream, StreamingJob>();
            _tunningPath = null;
        }

        internal void EnableTunnig(string path)
        {
            _tunningPath = path;
        }

        // Max duration of audio ~60s (https://cloud.google.com/speech/limits)
        internal override async Task<SpeechRecognitionAlternative> SyncRecognition(string audio_base64, string encoding, 
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            using (SyncJob job = new SyncJob(audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts))
            {
                await job.StartAsync(_recognizer);

                return job.BestAlternative;
            }
        }

        internal override async Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding, 
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            StreamingJob job = new StreamingJob(streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);

            lock (_streamingJobs)
                _streamingJobs.Add(streamIn, job);

            await job.StartAsync(_recognizer);
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

            await job.Stop();

            SpeechRecognitionAlternative result = job.BestAlternative;

            lock (_streamingJobs)
                _streamingJobs.Remove(streamIn);

            job.Dispose();

            return result;
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

        internal static int GetBytesPerSample(string encoding)
        {
            var enc = ConvertAudioEncoding(encoding);

            switch(enc)
            {
                case AudioEncoding.Flac:
                case AudioEncoding.Linear16:
                    return 2;

                case AudioEncoding.Mulaw:
                    return 1;

                default:
                    return 0;
            }
        }
    }
}
