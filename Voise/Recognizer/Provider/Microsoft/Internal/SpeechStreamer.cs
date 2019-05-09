using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Voise.Recognizer.Provider.Microsoft.Internal
{
    // This sets up a circular buffer that never finishes allowing realtime processing of speech.
    // The problem with normal streams is that if they return a fixed length or less bytes than 
    // requested the speech recogniser assumes that the stream has finished.
    internal class SpeechStreamer : Stream
    {
        [Serializable]
        public class BufferOverwrittenException : System.Exception
        {
            internal BufferOverwrittenException()
                : base()
            {
            }
        }

        /// <summary>
        /// Object for synchronization between read and write
        /// </summary>
        private AutoResetEvent _writeEvent;
        private object _writeEventObject;

        /// <summary>
        /// Buffer containing the stream data
        /// </summary>
        private readonly List<byte> _buffer;
        private readonly int _buffersize;

        private int _readposition;
        private int _writeposition;
        private bool _reset;

        public SpeechStreamer(int bufferSize)
        {
            _writeEvent = new AutoResetEvent(false);
            _writeEventObject = new object();

            _buffersize = bufferSize;
            _buffer = new List<byte>(_buffersize);

            for (int i = 0; i < _buffersize; i++)
                _buffer.Add(new byte());

            _readposition = 0;
            _writeposition = 0;
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
            int i = 0;
            while (i < count)
            {
                lock (_writeEventObject)
                {
                    if (!_reset && _readposition >= _writeposition)
                    {
                        // Whether was completed...
                        if (_writeEvent == null)
                            break;

                        _writeEvent.WaitOne(50, true);
                        continue;
                    }
                }

                buffer[i] = _buffer[_readposition + offset];
                _readposition++;

                if (_readposition == _buffersize)
                {
                    _readposition = 0;
                    _reset = false;
                }

                i++;
            }

            return i;
        }

        /// <summary>
        /// Writes an array of bytes to the stream
        /// </summary>
        /// <param name="buffer">array containing the bytes that will be written</param>
        /// <param name="offset">Index of the first byte from the array that will be written</param>
        /// <param name="count">Amount of bytes that will be written</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            bool overwritten = false;

            for (int i = offset; i < offset + count; i++)
            {
                _buffer[_writeposition] = buffer[i];
                _writeposition++;

                if (_writeposition == _readposition)
                    overwritten = true;

                if (_writeposition == _buffersize)
                {
                    _writeposition = 0;
                    _reset = true;
                }
            }

            _writeEvent.Set();

            if (overwritten)
                throw new BufferOverwrittenException();
        }

        /// <summary>
        /// Indicates that the write finished
        /// </summary>
        public void Complete()
        {
            lock (_writeEventObject)
            {
                if (_writeEvent != null)
                {
                    _writeEvent.Close();
                    _writeEvent = null;
                }
            }
        }

        public override void Close()
        {
            Complete();
            base.Close();
        }

        public override void Flush()
        {
        }
    }
}
