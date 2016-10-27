using Google.Longrunning;
using System;

namespace Voise.Google.Longrunning
{
    /// <summary>
    /// An exception to indicate that a long-running operation failed.
    /// </summary>
    public class OperationFailedException : Exception
    {
        /// <summary>
        /// The operation message containing the original error.
        /// </summary>
        public Operation Operation { get; }

        public OperationFailedException(Operation proto) : base(proto.Error.Message)
        {
            // TODO: Validation etc.
            Operation = proto;
        }
    }
}
