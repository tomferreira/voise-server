using System.Collections.Generic;
using Voise.Recognizer.Provider.Azure.Job;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Azure
{
    // Para mais informações leia: https://docs.microsoft.com/pt-pt/azure/cognitive-services/speech/getstarted/getstartedcsharpdesktop
    internal sealed class AzureRecognizer : CommonRecognizer
    {
        internal const string ENGINE_IDENTIFIER = "ze";

        private string _primaryKey;

        internal AzureRecognizer(string primaryKey)
        {
            if (primaryKey == null)
                throw new System.Exception("Primary key must be defined for Azure engine.");

            _primaryKey = primaryKey;
        }

        protected override ISyncJob CreateSyncJob(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(_primaryKey, audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode);
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