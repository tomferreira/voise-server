using System.Collections.Generic;
using Voise.General;
using Voise.Provider.Microsoft;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Microsoft.Job;

namespace Voise.Recognizer.Provider.Microsoft
{
    internal sealed class MicrosoftRecognizer : CommonRecognizer
    {
        internal const string ENGINE_IDENTIFIER = "me";

        protected override ISyncJob CreateSyncJob(byte[] audio, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(audio, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
        }

        protected override IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new StreamingJob(streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
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
                    return AudioEncoding.Alaw;

                case "mulaw":
                    return AudioEncoding.Mulaw;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}
