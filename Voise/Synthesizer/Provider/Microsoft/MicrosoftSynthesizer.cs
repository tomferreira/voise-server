using Voise.General;
using Voise.Provider.Microsoft;
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

        private AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case Constant.ENCODING_FLAC:
                    throw new System.Exception($"Codec '{Constant.ENCODING_FLAC}' not supported.");

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
