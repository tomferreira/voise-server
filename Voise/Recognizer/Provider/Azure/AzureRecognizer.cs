using System.Collections.Generic;
using Voise.General;
using Voise.Recognizer.Provider.Azure.Job;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Azure
{
    // Para mais informações leia: https://docs.microsoft.com/pt-pt/azure/cognitive-services/speech/getstarted/getstartedcsharpdesktop
    internal sealed class AzureRecognizer : CommonRecognizer
    {
        internal const string ENGINE_IDENTIFIER = "ze";

        private readonly string _primaryKey;

        internal AzureRecognizer(string primaryKey)
        {
            if (primaryKey == null)
                throw new System.Exception("Primary key must be defined for Azure engine.");

            _primaryKey = primaryKey;
        }

        protected override ISyncJob CreateSyncJob(byte[] audio, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(_primaryKey, audio, ConvertAudioEncoding(encoding), sampleRate, languageCode);
        }

        protected override IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new StreamingJob(_primaryKey, streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode);
        }

        private AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case Constant.ENCODING_FLAC:
                    throw new System.Exception($"Codec '{Constant.ENCODING_FLAC}' not supported.");

                case Constant.ENCODING_LINEAR16:
                    return AudioEncoding.Linear16;

                case Constant.ENCODING_ALAW:
                    throw new System.Exception($"Codec '{Constant.ENCODING_ALAW}' not supported.");

                case Constant.ENCODING_MULAW:
                    throw new System.Exception($"Codec '{Constant.ENCODING_MULAW}' not supported.");

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}