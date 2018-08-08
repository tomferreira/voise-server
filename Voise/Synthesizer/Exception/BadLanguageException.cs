using System;
using System.Runtime.Serialization;

namespace Voise.Synthesizer.Exception
{
    [Serializable]
    internal class BadLanguageException : System.Exception
    {
        public BadLanguageException(string message)
            : base(message)
        {
        }

        public BadLanguageException(string message, System.Exception innerException) 
            : base(message, innerException)
        {
        }

        protected BadLanguageException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}