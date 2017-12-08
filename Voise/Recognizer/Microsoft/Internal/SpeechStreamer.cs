using System.IO;
using System.Threading;

namespace Voise.Recognizer.Microsoft.Internal
{
    // This sets up a circular buffer that never finishes allowing realtime processing of speech.
    // The problem with normal streams is that if they return a fixed length or less bytes than 
    // requested the speech recogniser assumes that the stream has finished.
    internal class SpeechStreamer : Stream
    {
        /// <summary>
        /// Object for synchronization between read and write
        /// </summary>
        private AutoResetEvent _writeEvent;

        /// <summary>
        /// Buffer containing the stream data
        /// </summary>
        private CircularBuffer<byte> _buffer;

        private bool _completed;
        private object _monitorCompleted;

        public SpeechStreamer(int bufferSize)
        {
            _buffer = new CircularBuffer<byte>(bufferSize);
            _writeEvent = new AutoResetEvent(false);

            _monitorCompleted = new object();
            _completed = false;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return -1L; }
        }

        public override long Position
        {
            get { return 0L; }
            set { }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 0L;
        }

        public override void SetLength(long value)
        {
        }

        /// <summary>
        /// Reads a determined amount of bytes and copy them inside the buffer parameter, returns count value
        /// </summary>
        /// <param name="buffer">Array where the read bytes will be copied</param>
        /// <param name="offset">Index of the array where the first element will be copied</param>
        /// <param name="count">Amount of bytes that will be read</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            while (true)
            {
                if (_buffer.TryGet(buffer, offset, count))
                    return count;

                // If completed, reads the rest of buffer
                lock (_monitorCompleted)
                {
                    if (_completed)
                    {
                        count = _buffer.Count;
                        _buffer.TryGet(buffer, offset, _buffer.Count);

                        return count;
                    }
                }

                // If not closed, wait for a buffered write
                _writeEvent.WaitOne(50, true);
                continue;
            }
        }

        /// <summary>
        /// Writes an array of bytes to the stream
        /// </summary>
        /// <param name="buffer">array containing the bytes that will be written</param>
        /// <param name="offset">Index of the first byte from the array that will be written</param>
        /// <param name="count">Amount of bytes that will be written</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_buffer.TryAdd(buffer, offset, count))
                throw new EndOfStreamException("Can not write more values to the Circular stream!");

            _writeEvent.Set();
        }

        /// <summary>
        /// Indicates that the write finished
        /// </summary>
        public void Complete()
        {
            lock (_monitorCompleted)
                _completed = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writeEvent.Close();
                _buffer.Dispose();
            }

            base.Dispose(disposing);
        }

        public override void Flush()
        {
        }
    }
}
