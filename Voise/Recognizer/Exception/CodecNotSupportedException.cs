﻿using System;

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
    }
}
