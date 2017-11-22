﻿using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Recognizer.Azure.Job;
using Voise.Synthesizer.Azure;

namespace Voise.Recognizer.Azure
{
    // Para mais informações leia: https://docs.microsoft.com/pt-pt/azure/cognitive-services/speech/getstarted/getstartedcsharpdesktop
    internal sealed class AzureRecognizer : Base
    {
        internal const string ENGINE_IDENTIFIER = "ze";

        private Dictionary<AudioStream, StreamingJob> _streamingJobs;

        internal AzureRecognizer()
        {
            _streamingJobs = new Dictionary<AudioStream, StreamingJob>();
        }

        internal override async Task<SpeechRecognitionAlternative> SyncRecognition(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            SyncJob job = new SyncJob(audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode);

            job.Start();

            return job.BestAlternative;
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

            lock (_streamingJobs)
                _streamingJobs.Remove(streamIn);

            return job.BestAlternative;
        }

        private AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case "alaw":
                    throw new System.Exception("Enconding 'alaw' not supported.");

                case "mulaw":
                    throw new System.Exception("Enconding 'mulaw' not supported.");

                case "linear16":
                    return AudioEncoding.Linear16;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}