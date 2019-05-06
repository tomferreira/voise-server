using CPqDAsr.ASR;
using CPqDASR.Communication;
using CPqDASR.Config;
using System.Collections.Generic;
using Voise.General;
using Voise.Recognizer.Exception;
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

        protected override ISyncJob CreateSyncJob(byte[] audio, string encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(_config, _modelList, audio, ConvertAudioEncoding(encoding), sampleRate, languageCode);
        }

        protected override IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new StreamingJob(_config, streamIn, _modelList, ConvertAudioEncoding(encoding), sampleRate, languageCode);
        }

        private static AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToUpperInvariant())
            {
                case Constant.ENCODING_FLAC:
                    throw new CodecNotSupportedException($"Codec '{Constant.ENCODING_FLAC}' not supported.");

                case Constant.ENCODING_LINEAR16:
                    return AudioEncoding.Linear16;

                case Constant.ENCODING_ALAW:
                    throw new CodecNotSupportedException($"Codec '{Constant.ENCODING_ALAW}' not supported.");

                case Constant.ENCODING_MULAW:
                    throw new CodecNotSupportedException($"Codec '{Constant.ENCODING_MULAW}' not supported.");

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}