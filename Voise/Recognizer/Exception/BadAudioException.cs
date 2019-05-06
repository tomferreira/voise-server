using System;
using System.Runtime.Serialization;

namespace Voise.Recognizer.Exception
{
    [Serializable]
    public class BadAudioException : System.Exception
    {
        public BadAudioException()
        {
        }

        public BadAudioException(string message)
            : base(message)
        {
        }

        public BadAudioException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected BadAudioException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
