﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Voise
{
    internal class AudioStream
    {
        /// <summary>
        /// Event Args for StreamIn event
        /// </summary>
        internal class StreamInEventArgs : EventArgs
        {
            private byte[] buffer;
            private int bytes;

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

        /// <summary>
        /// Indicates streamed data is available 
        /// </summary>
        internal event EventHandler<StreamInEventArgs> DataAvailable;

        /// <summary>
        /// Indicates that all streamed data has now been received.
        /// </summary>
        internal event EventHandler StreamingStopped;

        private bool _started;

        private Queue<MemoryStream> _buffers;
        private MemoryStream _currentBuffer;

        internal int BufferCapacity { get; private set; }

        internal AudioStream(int bufferMillisec, int sampleRate, int bytesPerSample)
        {
            int bytesPerSecond = sampleRate * bytesPerSample;

            _started = false;

            BufferCapacity = bufferMillisec * bytesPerSecond / 1000;            
            _buffers = new Queue<MemoryStream>();

            CreateBuffer();
        }

        internal void Start()
        {
            _started = true;
        }

        internal void Stop()
        {
            if (!_started)
                return;

            _started = false;

            // Enqueue current buffer to be send too
            if (_currentBuffer != null)
            {
                lock(_buffers)
                    _buffers.Enqueue(_currentBuffer);
            }

            Callback(true);

            StreamingStopped?.Invoke(this, EventArgs.Empty);
        }

        internal void Write(byte[] data)
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

                offset += count;
                length -= count;
            }

            if (_started)
                Callback();
        }

        private void Callback(bool sendNotFull = false)
        {
            lock (_buffers)
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
            if (_currentBuffer != null)
            {
                lock(_buffers)
                    _buffers.Enqueue(_currentBuffer);
            }

            _currentBuffer = new MemoryStream(BufferCapacity);            
        }
    }
}