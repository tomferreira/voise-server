using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Voise.Recognizer.Microsoft
{
    // This sets up a circular buffer that never finishes allowing realtime processing of speech.
    // The problem with normal streams is that if they return a fixed length or less bytes than 
    // requested the speech recogniser assumes that the stream has finished.
    internal class SpeechStreamer : Stream
    {
        private AutoResetEvent _writeEvent;
        private List<byte> _buffer;
        private int _buffersize;
        private int _readposition;
        private int _writeposition;
        private bool _reset;

        public SpeechStreamer(int bufferSize)
        {
            _writeEvent = new AutoResetEvent(false);

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

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Stream closed
            if (_writeEvent == null)
                return 0;

            int i = 0;
            while (i < count && _writeEvent != null)
            {
                if (!_reset && _readposition >= _writeposition)
                {
                    _writeEvent.WaitOne(100, true);
                    continue;
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

            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (int i = offset; i < offset + count; i++)
            {
                _buffer[_writeposition] = buffer[i];
                _writeposition++;

                if (_writeposition == _buffersize)
                {
                    _writeposition = 0;
                    _reset = true;
                }
            }

            _writeEvent.Set();
        }

        public override void Close()
        {
            _writeEvent.Close();
            _writeEvent = null;

            base.Close();
        }

        public override void Flush()
        {
        }
    }
}
