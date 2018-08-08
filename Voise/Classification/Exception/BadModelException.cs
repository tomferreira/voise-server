using System;
using System.Runtime.Serialization;

namespace Voise.Classification.Exception
{
    [Serializable]
    internal class BadModelException : System.Exception
    {
        public BadModelException(string message)
            : base(message)
        {
        }

        public BadModelException(string message, System.Exception innerException) 
            : base(message, innerException)
        {
        }

        protected BadModelException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
