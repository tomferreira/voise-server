using System;
using System.Runtime.Serialization;

namespace Voise.Classification.Exception
{
    [Serializable]
    public class ClassifiersPathNotFoundException : System.Exception
    {
        public ClassifiersPathNotFoundException()
        {
        }

        public ClassifiersPathNotFoundException(string message)
            : base(message)
        {
        }

        public ClassifiersPathNotFoundException(string message, System.Exception innerException) 
            : base(message, innerException)
        {
        }

        protected ClassifiersPathNotFoundException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
