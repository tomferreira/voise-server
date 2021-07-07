using System;
using System.IO;

namespace Voise.Synthesizer.Provider.Microsoft
{
    internal class WaveStream : MemoryStream
    {
        internal class ProgressEventArgs
        {
            internal byte[] Chunk { get; private set; }

            internal ProgressEventArgs(byte[] chunk)
            {
                Chunk = chunk;
            }
        }

        internal event EventHandler<ProgressEventArgs> Progress;

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);

            if (Progress != null)
            {
                byte[] buffer2 = new byte[count];
                Array.Copy(buffer, offset, buffer2, 0, count);

                Progress.Invoke(this, new ProgressEventArgs(buffer2));
            }
        }
    }
}
