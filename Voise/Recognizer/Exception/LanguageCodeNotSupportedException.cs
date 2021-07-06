using System;
using System.Runtime.Serialization;

namespace Voise.Recognizer.Exception
{
    [Serializable]
    public class LanguageCodeNotSupportedException : System.Exception
    {
        public LanguageCodeNotSupportedException()
        {
        }

        public LanguageCodeNotSupportedException(string message)
            : base(message)
        {
        }

        public LanguageCodeNotSupportedException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected LanguageCodeNotSupportedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
