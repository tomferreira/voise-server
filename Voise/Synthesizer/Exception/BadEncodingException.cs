
using System;

namespace Voise.Synthesizer.Exception
{
    [Serializable]
    class BadEncodingException : System.Exception
    {
        internal BadEncodingException(string message)
            : base(message)
        {
        }
    }
}