using System;
using System.Runtime.Serialization;

namespace Voise.Synthesizer.Exception
{
    [Serializable]
    public class BadVoiceException : System.Exception
    {
        public BadVoiceException()
        {
        }

        public BadVoiceException(string message)
            : base(message)
        {
        }

        public BadVoiceException(string message, System.Exception innerException) 
            : base(message, innerException)
        {
        }

        protected BadVoiceException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}