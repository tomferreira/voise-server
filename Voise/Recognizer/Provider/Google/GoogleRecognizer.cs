using System.Collections.Generic;
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

        private SpeechRecognizer _recognizer;

        internal GoogleRecognizer(string credentialPath)
        {
            if (credentialPath == null)
                throw new System.Exception("Credential path must be defined for Google engine.");

            _recognizer = SpeechRecognizer.Create(credentialPath);
        }

        // Max duration of audio ~60s (https://cloud.google.com/speech/limits)
        protected override ISyncJob CreateSyncJob(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(_recognizer, audio_base64, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
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
                case "flac":
                    return AudioEncoding.Flac;

                case "linear16":
                    return AudioEncoding.Linear16;

                case "alaw":
                    throw new System.Exception("Codec 'alaw' not supported.");

                case "mulaw":
                    return AudioEncoding.Mulaw;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }

        internal static int GetBytesPerSample(string encoding)
        {
            var enc = ConvertAudioEncoding(encoding);

            switch (enc)
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
