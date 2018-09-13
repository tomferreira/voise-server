using System.Collections.Generic;
using Voise.General;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Google.Internal;
using Voise.Recognizer.Provider.Google.Job;
using static Google.Cloud.Speech.V1.RecognitionConfig.Types;

namespace Voise.Recognizer.Provider.Google
{
    internal sealed class GoogleRecognizer : CommonRecognizer
    {
        internal const string ENGINE_IDENTIFIER = "ge";

        private readonly SpeechRecognizer _recognizer;

        internal GoogleRecognizer(string credentialPath)
        {
            if (credentialPath == null)
                throw new System.Exception("Credential path must be defined for Google engine.");

            _recognizer = SpeechRecognizer.Create(credentialPath);
        }

        // Max duration of audio ~60s (https://cloud.google.com/speech/limits)
        protected override ISyncJob CreateSyncJob(byte[] audio, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(_recognizer, audio, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
        }

        protected override IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new StreamingJob(_recognizer, streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
        }

        internal static AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case Constant.ENCODING_FLAC:
                    return AudioEncoding.Flac;

                case Constant.ENCODING_LINEAR16:
                    return AudioEncoding.Linear16;

                case Constant.ENCODING_ALAW:
                    throw new System.Exception($"Codec '{Constant.ENCODING_ALAW}' not supported.");

                case Constant.ENCODING_MULAW:
                    return AudioEncoding.Mulaw;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}
