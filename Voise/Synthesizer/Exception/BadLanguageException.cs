using System;

namespace Voise.Synthesizer.Exception
{
    [Serializable]
    class BadLanguageException : System.Exception
    {
        internal BadLanguageException(string message)
            : base(message)
        {
        }
    }
}