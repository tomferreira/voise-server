using System;
using System.Runtime.Serialization;

namespace Voise.Classification.Exception
{
    [Serializable]
    internal class ClassifiersPathNotFound : System.Exception
    {
        public ClassifiersPathNotFound(string message)
            : base(message)
        {
        }

        public ClassifiersPathNotFound(string message, System.Exception innerException) 
            : base(message, innerException)
        {
        }

        protected ClassifiersPathNotFound(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
