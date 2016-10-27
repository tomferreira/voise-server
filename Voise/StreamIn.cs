using System;
using System.Collections.Generic;
using System.IO;

namespace Voise
{
    internal class StreamIn
    {
        /// <summary>
        /// Event Args for StreamIn event
        /// </summary>
        public class StreamInEventArgs : EventArgs
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
        public event EventHandler<StreamInEventArgs> DataAvailable;

        /// <summary>
        /// Indicates that all streamed data has now been received.
        /// </summary>
        public event EventHandler StreamingStopped;

        private bool _started;
        private int _bufferSize;
        private Queue<MemoryStream> _buffers;
        private MemoryStream _currentBuffer;

        internal StreamIn(int bufferMillisec, int sampleRate, int bytesPerSample)
        {
            int bytesPerSecond = sampleRate * bytesPerSample;

            _started = false;
            
            _bufferSize = bufferMillisec * bytesPerSecond / 1000;            
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
                _buffers.Enqueue(_currentBuffer);

            Callback();

            StreamingStopped?.Invoke(this, EventArgs.Empty);
        }

        internal void AddData(byte[] data)
        {
            if (_currentBuffer.Length >= _currentBuffer.Capacity)
                CreateBuffer();

            _currentBuffer.Write(data, 0, data.Length);

            if (_started)
                Callback();
        }

        private void Callback()
        {            
            while (_buffers.Count > 0)
            {
                MemoryStream buffer = _buffers.Dequeue();

                DataAvailable?.Invoke(this, new StreamInEventArgs(buffer.GetBuffer(), Convert.ToInt32(buffer.Length)));
            }
        }

        private void CreateBuffer()
        {
            if (_currentBuffer != null)
                _buffers.Enqueue(_currentBuffer);

            _currentBuffer = new MemoryStream(_bufferSize);            
        }

    }
}
