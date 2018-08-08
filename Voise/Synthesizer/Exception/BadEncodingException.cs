
using System;
using System.Runtime.Serialization;

namespace Voise.Synthesizer.Exception
{
    [Serializable]
    internal class BadEncodingException : System.Exception
    {
        public BadEncodingException(string message)
            : base(message)
        {
        }

        public BadEncodingException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected BadEncodingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}