using Voise.General;
using Voise.Provider.Microsoft;
using Voise.Synthesizer.Exception;
using Voise.Synthesizer.Provider.Common;
using Voise.Synthesizer.Provider.Common.Job;

namespace Voise.Synthesizer.Provider.Microsoft
{
    internal class MicrosoftSynthesizer : CommonSynthesizer
    {
        internal const string ENGINE_IDENTIFIER = "me";

        protected override IJob CreateJob(AudioStream streamOut, string encoding,
            int sampleRate, string languageCode)
        {
            return new Job(streamOut, ConvertAudioEncoding(encoding), sampleRate, languageCode);
        }

        internal override int GetBytesPerSample(string encoding)
        {
            try
            {
                return ConvertAudioEncoding(encoding).BitsPerSample / 8;
            }
            catch
            {
                return 1;
            }
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
