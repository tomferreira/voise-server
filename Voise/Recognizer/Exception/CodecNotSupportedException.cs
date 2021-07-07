using System;
using System.Runtime.Serialization;

namespace Voise.Recognizer.Exception
{
    [Serializable]
    public class CodecNotSupportedException : System.Exception
    {
        public CodecNotSupportedException()
        {
        }

        public CodecNotSupportedException(string message)
            : base(message)
        {
        }

        public CodecNotSupportedException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected CodecNotSupportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
