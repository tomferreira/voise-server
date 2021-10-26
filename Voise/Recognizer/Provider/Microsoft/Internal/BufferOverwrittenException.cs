using System;
using System.Runtime.Serialization;

namespace Voise.Recognizer.Provider.Microsoft.Internal
{
    [Serializable]
    public class BufferOverwrittenException : System.Exception
    {
        public BufferOverwrittenException()
            : base()
        {
        }

        public BufferOverwrittenException(string message)
            : base(message)
        {
        }

        public BufferOverwrittenException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected BufferOverwrittenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
