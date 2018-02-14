using CPqDAsr.ASR;
using CPqDASR.Communication;
using CPqDASR.Config;
using System.Collections.Generic;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Cpqd.Job;

namespace Voise.Recognizer.Provider.Cpqd
{
    internal class CpqdRecognizer : CommonRecognizer
    {
        internal const string ENGINE_IDENTIFIER = "ce";

        private readonly ClientConfig _config;
        private readonly LanguageModelList _modelList;

        public CpqdRecognizer(string username, string password, string host)
        {
            _config = new ClientConfig
            {
                ServerUrl = host,
                Credentials = new Credentials
                {
                    UserName = username,
                    Password = password
                },
                RecogConfig = new RecognitionConfig
                {
                    MaxSentences = 1,
                    RecognitionTimeoutEnabled = false,
                    ConfidenceThreshold = 0,
                    NoInputTimeoutEnabled = false,
                    ContinuousMode = true
                },
                ConnectOnRecognize = false,
                AutoClose = false
            };

            _modelList = new LanguageModelList();
            _modelList.AddFromUri("builtin:slm/general");
        }

        protected override ISyncJob CreateSyncJob(string audio_base64, string encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(_config, _modelList, audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode);
        }

        protected override IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new StreamingJob(_config, streamIn, _modelList, ConvertAudioEncoding(encoding), sampleRate, languageCode);
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
                    throw new System.Exception("Codec 'alaw' not supported.");

                case "mulaw":
                    throw new System.Exception("Codec 'mulaw' not supported.");

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}