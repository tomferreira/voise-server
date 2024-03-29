﻿using Microsoft.Speech.AudioFormat;
using Voise.General;

namespace Voise.Provider.Microsoft
{
    internal class AudioEncoding
    {
        internal static readonly AudioEncoding Alaw = new AudioEncoding(EncodingFormat.ALaw, 8, Constant.CHANNEL_MONO, 1);
        internal static readonly AudioEncoding Mulaw = new AudioEncoding(EncodingFormat.ULaw, 8, Constant.CHANNEL_MONO, 1);
        internal static readonly AudioEncoding Linear16 = new AudioEncoding(EncodingFormat.Pcm, 16, Constant.CHANNEL_MONO, 2);
        internal static readonly AudioEncoding EncodingUnspecified = null;

        internal EncodingFormat Format { get; private set; }
        internal int BitsPerSample { get; private set; }
        internal int ChannelCount { get; private set; }
        internal int BlockAlign { get; private set; }

        private AudioEncoding(EncodingFormat format, int bitsPerSample, int channelCount, int blockAlign)
        {
            Format = format;
            BitsPerSample = bitsPerSample;
            ChannelCount = channelCount;
            BlockAlign = blockAlign;
        }
    }
}
