﻿using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Recognizer.Microsoft.Job;
using Voise.Synthesizer.Microsoft;

namespace Voise.Recognizer.Microsoft
{
    internal class MicrosoftRecognizer : Base
    {
        private Dictionary<AudioStream, StreamingJob> _streamingJobs;

        internal MicrosoftRecognizer()
        {
            _streamingJobs = new Dictionary<AudioStream, StreamingJob>();
        }

        internal override async Task<SpeechRecognitionAlternative> SyncRecognition(string audio_base64, string encoding,
            int sampleRate, string languageCode, List<string> context)
        {
            SyncJob job = new SyncJob(audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode);

            job.Start();

            return job.BestAlternative;
        }

        internal override async Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, List<string> context)
        {
            StreamingJob job = new StreamingJob(streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode, context);
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
