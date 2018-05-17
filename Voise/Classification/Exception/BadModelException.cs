using System;

namespace Voise.Classification.Exception
{
    [Serializable]
    internal class BadModelException : System.Exception
    {
        internal BadModelException(string message)
            : base(message)
        {
        }
    }
}
