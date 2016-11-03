﻿using System.Speech.AudioFormat;

namespace Voise.Synthesizer.Microsoft
{
    internal class AudioEncoding
    {
        internal static readonly AudioEncoding Alaw = new AudioEncoding(EncodingFormat.ALaw, 8, 1);
        internal static readonly AudioEncoding Mulaw = new AudioEncoding(EncodingFormat.ULaw, 8, 1);
        internal static readonly AudioEncoding Linear16 = new AudioEncoding(EncodingFormat.Pcm, 16, 1);
        internal static readonly AudioEncoding EncodingUnspecified = null;

        internal EncodingFormat Format { get; private set; }
        internal int BitsPerSample { get; private set; }
        internal int ChannelCount { get; private set; }

        private AudioEncoding(EncodingFormat format, int bitsPerSample, int channelCount)
        {
            Format = format;
            BitsPerSample = bitsPerSample;
            ChannelCount = channelCount;
        }
    }
}
