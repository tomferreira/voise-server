using System.Collections.Generic;
using System.Threading;
using CPqDASR.ASR;

namespace Voise.Recognizer.Provider.Cpqd.Internal
{
    public class BufferAudioSource : IAudioSource
    {
        private Queue<byte[]> _buffer;
        private bool _finished;
        private object _monitorFinished;

        private AutoResetEvent _writeEvent;

        public BufferAudioSource()
        {
            _writeEvent = new AutoResetEvent(false);
            _buffer = new Queue<byte[]>();

            _monitorFinished = new object();
        }

        public BufferAudioSource(byte[] bytes) : this()
        {
            _buffer.Enqueue(bytes);
        }

        public bool Write(byte[] bytes)
        {
            lock (_monitorFinished)
            {
                if (!_finished)
                {
                    _buffer.Enqueue(bytes);
                    _writeEvent.Set();

                    return true;
                }
            }

            return false;
        }

        public byte[] Read()
        {
            while (true)
            {
                lock (_monitorFinished)
                {
                    if (_finished)
                        return new byte[0];

                    if (_buffer.Count > 0)
                        return _buffer.Dequeue();
                }

                // If not closed, wait for a buffered write
                _writeEvent.WaitOne(50, true);
            }
        }

        public void Close()
        {
            _buffer = null;
        }

        public void Finish()
        {
            lock (_monitorFinished)
            {
                _finished = true;
                Close();
            }
        }
    }
}