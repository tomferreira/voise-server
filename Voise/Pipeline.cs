using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Voise
{
    internal class Pipeline
    {
        private static long _nextId = 1;
        private static object _lockNextId = new object();

        private SemaphoreSlim _mutex;

        // Identification of pipeline
        internal long Id { get; private set; }

        // Recognizer used in this pipeline (if ASR)
        internal Recognizer.Base Recognizer { get; set; }

        internal SpeechResult SpeechResult { get; set; }

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
            await _mutex.WaitAsync();
        }

        internal void ReleaseMutex()
        {
            _mutex.Release();
        }
    }
}
