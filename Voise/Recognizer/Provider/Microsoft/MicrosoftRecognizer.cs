using System.Collections.Generic;
using Voise.General;
using Voise.General.Interface;
using Voise.Provider.Microsoft;
using Voise.Recognizer.Exception;
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

        protected override IStreamingJob CreateStreamingJob(IAudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new StreamingJob(streamIn, ConvertAudioEncoding(encoding), sampleRate, languageCode, contexts);
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
                    return AudioEncoding.Alaw;

                case Constant.ENCODING_MULAW:
                    return AudioEncoding.Mulaw;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}
