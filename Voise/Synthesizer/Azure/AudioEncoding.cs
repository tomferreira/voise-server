using Microsoft.CognitiveServices.SpeechRecognition;

namespace Voise.Synthesizer.Azure
{
    internal class AudioEncoding
    {
        internal static readonly AudioEncoding Linear16 = new AudioEncoding(AudioCompressionType.PCM, 16, 1, 2);
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
