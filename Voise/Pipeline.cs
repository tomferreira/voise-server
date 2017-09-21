using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Voise
{
    internal class Pipeline
    {
        private SemaphoreSlim _mutex;

        // Recognizer used in this pipeline (if ASR)
        internal Recognizer.Base Recognizer { get; set; }

        internal SpeechResult SpeechResult { get; set; }

        // Internal async error
        internal Exception AsyncStreamError { get; set; }

        internal Pipeline()
        {
            _mutex = new SemaphoreSlim(0);
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
