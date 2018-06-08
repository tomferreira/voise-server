using Voise.Provider.Microsoft;
using Voise.Synthesizer.Provider.Common;
using Voise.Synthesizer.Provider.Common.Job;

namespace Voise.Synthesizer.Provider.Microsoft
{
    internal class MicrosoftSynthetizer : CommonSynthetizer
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
