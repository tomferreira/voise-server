using System;

namespace Voise.Recognizer.Exception
{
    [Serializable]
    internal class BadEncodingException : System.Exception
    {
        internal BadEncodingException(string message)
            : base(message)
        {
        }
    }
}
