﻿namespace Voise
{
    internal class SpeechResult
    {
        internal enum Modes
        {
            ASR,
            TTS
        };

        internal Modes Mode { get; private set; }

        // ASR
        internal string Transcript { get; set; }
        internal float Confidence { get; set; }
        internal string Intent { get; set; }
        internal float Probability { get; set; }

        // TTS
        internal string AudioContent { get; set; } // Base64 encoded

        internal SpeechResult(Modes mode)
        {
            Mode = mode;
        }
    }
}
