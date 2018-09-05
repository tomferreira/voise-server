using System;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common;

namespace Voise
{
    internal class Pipeline : IDisposable
    {
        private static long _nextId = 1;
        private static object _lockNextId = new object();

        private SemaphoreSlim _mutex;

        // Identification of pipeline
        internal long Id { get; private set; }

        // Recognizer used in this pipeline (if ASR)
        internal CommonRecognizer Recognizer { get; set; }

        internal VoiseResult Result { get; set; }

        // Internal async error
        internal Exception AsyncStreamError { get; set; }

        internal Pipeline()
        {
            _mutex = new SemaphoreSlim(0);

            lock (_lockNextId)
                Id = _nextId++;
        }

        internal async Task WaitAsync()
        {
            await _mutex.WaitAsync().ConfigureAwait(false);
        }

        internal void ReleaseMutex()
        {
            _mutex.Release();
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
                _mutex.Dispose();
            }
        }
    }
}
