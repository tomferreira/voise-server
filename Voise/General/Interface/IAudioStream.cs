using System;

namespace Voise.General.Interface
{
    public interface IAudioStream : IDisposable
    {
        /// <summary>
        /// Indicates streamed data is available 
        /// </summary>
        event EventHandler<IStreamInEventArgs> DataAvailable;

        /// <summary>
        /// Indicates that all streamed data has now been received.
        /// </summary>
        event EventHandler StreamingStopped;

        int BufferCapacity { get; }

        void Start();
        void Stop();

        void Abort();

        bool IsStarted();

        bool IsAborted();

        void Write(byte[] data);
    }
}
