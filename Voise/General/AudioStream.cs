using System;
using System.Collections.Generic;
using System.IO;
using Voise.General.Interface;

namespace Voise.General
{
    internal class AudioStream : IAudioStream
    {
        /// <summary>
        /// Event Args for StreamIn event
        /// </summary>
        internal class StreamInEventArgs : EventArgs, IStreamInEventArgs
        {
            private readonly byte[] buffer;
            private readonly int bytes;

            /// <summary>
            /// Creates new StreamInEventArgs
            /// </summary>
            public StreamInEventArgs(byte[] buffer, int bytes)
            {
                this.buffer = buffer;
                this.bytes = bytes;
            }

            /// <summary>
            /// Buffer containing streamed data. Note that it might not be completely
            /// full. <seealso cref="BytesStreamed"/>
            /// </summary>
            public byte[] Buffer
            {
                get { return buffer; }
            }

            /// <summary>
            /// The number of streamed bytes in Buffer. <seealso cref="Buffer"/>
            /// </summary>
            public int BytesStreamed
            {
                get { return bytes; }
            }
        }

        enum State
        {
            Started,
            Stopped,
            Aborted
        }

        private State _state;
        private readonly Queue<MemoryStream> _buffers;
        private MemoryStream _currentBuffer;

        private readonly Tuning.Base _tuning;

        private readonly object _mutex;

        public event EventHandler<IStreamInEventArgs> DataAvailable;
        public event EventHandler StreamingStopped;

        public int BufferCapacity { get; private set; }

        internal AudioStream(int maxFrameMillisec, int sampleRate, int bytesPerSample, Tuning.Base tuning)
        {
            _tuning = tuning;

            int bytesPerSecond = sampleRate * bytesPerSample;

            _state = State.Stopped;

            BufferCapacity = maxFrameMillisec * bytesPerSecond / 1000;
            _buffers = new Queue<MemoryStream>();

            _mutex = new object();

            CreateBuffer();
        }

        public void Start()
        {
            lock (_mutex)
                _state = State.Started;
        }

        public void Stop()
        {
            lock (_mutex)
            {
                if (!IsStarted())
                    return;

                _state = State.Stopped;

                // Enqueue current buffer to be send too
                if (_currentBuffer != null)
                    _buffers.Enqueue(_currentBuffer);
            }

            Callback(true);

            StreamingStopped?.Invoke(this, EventArgs.Empty);
        }

        public void Abort()
        {
            Stop();

            lock (_mutex)
                _state = State.Aborted;
        }

        public bool IsStarted()
        {
            lock (_mutex)
                return _state == State.Started;
        }

        public bool IsAborted()
        {
            lock (_mutex)
                return _state == State.Aborted;
        }

        public void Write(byte[] data)
        {
            lock (_mutex)
            {
                int offset = 0;
                int length = data.Length;

                while (length > 0)
                {
                    int remmaing = BufferCapacity - (int)_currentBuffer.Length;

                    if (remmaing == 0)
                    {
                        CreateBuffer();
                        continue;
                    }

                    int count = Math.Min(remmaing, length);
                    _currentBuffer.Write(data, offset, count);

                    //
                    _tuning?.WriteRecording(data, offset, count);

                    offset += count;
                    length -= count;
                }
            }

            if (IsStarted())
                Callback();
        }

        private void Callback(bool sendNotFull = false)
        {
            lock (_mutex)
            {
                while (_buffers.Count > 0)
                {
                    if (!sendNotFull && _buffers.Peek().Length < BufferCapacity)
                        return;

                    MemoryStream buffer = _buffers.Dequeue();

                    DataAvailable?.Invoke(this, new StreamInEventArgs(buffer.ToArray(), Convert.ToInt32(buffer.Length)));
                }
            }
        }

        private void CreateBuffer()
        {
            lock (_mutex)
            {
                if (_currentBuffer != null)
                    _buffers.Enqueue(_currentBuffer);

                _currentBuffer = new MemoryStream(BufferCapacity);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_mutex)
                    _currentBuffer?.Dispose();
            }
        }
    }
}
