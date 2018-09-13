using Microsoft.CognitiveServices.SpeechRecognition;
using Voise.General;

namespace Voise.Recognizer.Provider.Azure
{
    internal class AudioEncoding
    {
        // TODO: Add other encoding like Siren.
        internal static readonly AudioEncoding Linear16 = new AudioEncoding(AudioCompressionType.PCM, 16, Constant.CHANNEL_MONO, 2);
        internal static readonly AudioEncoding EncodingUnspecified = null;

        internal AudioCompressionType Format { get; private set; }

        internal short BitsPerSample { get; private set; }

        internal short ChannelCount { get; private set; }

        internal short BlockAlign { get; private set; }

        private AudioEncoding(AudioCompressionType format, short bitsPerSample, short channelCount, short blockAlign)
        {
            Format = format;
            BitsPerSample = bitsPerSample;
            ChannelCount = channelCount;
            BlockAlign = blockAlign;
        }
    }
}
