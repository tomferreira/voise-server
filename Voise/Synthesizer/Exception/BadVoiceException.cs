using System;

namespace Voise.Synthesizer.Exception
{
    [Serializable]
    class BadVoiceException : System.Exception
    {
        internal BadVoiceException(string message)
            : base(message)
        {
        }
    }
}