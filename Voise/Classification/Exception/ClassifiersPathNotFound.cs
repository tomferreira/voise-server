using System;

namespace Voise.Classification.Exception
{
    [Serializable]
    class ClassifiersPathNotFound : System.Exception
    {
        internal ClassifiersPathNotFound(string message)
            : base(message)
        {
        }
    }
}
