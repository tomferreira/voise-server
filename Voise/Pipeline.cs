using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Voise
{
    internal class Pipeline
    {
        private List<Func<Task>> _actions;

        private SemaphoreSlim _mutex;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _cancellationToken;

        // Recognizer used in this pipeline (if ASR)
        internal Recognizer.Base Recognizer { get; set; }

        internal SpeechResult SpeechResult { get; set; }

        // Internal async error
        internal Exception AsyncStreamError { get; set; }

        internal Pipeline()
        {
            _actions = new List<Func<Task>>();
            _mutex = new SemaphoreSlim(0);

            _tokenSource = new CancellationTokenSource();
            _cancellationToken = _tokenSource.Token;
        }

        internal void StartNew(Func<Task> action)
        {
            _actions.Add(action);
        }

        internal void CancelExecution()
        {
            _tokenSource.Cancel();
        }

        internal async Task WaitAsync()
        {
            await _mutex.WaitAsync();
        }

        internal void ReleaseMutex()
        {
            _mutex.Release();
        }

        internal async void WaitAll()
        {
            try
            {
                foreach(var action in _actions)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        _cancellationToken.ThrowIfCancellationRequested();

                    await action();
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
            finally
            {
                _actions.Clear();
                _actions = null;

                _tokenSource.Dispose();
                _tokenSource = null;

                _mutex.Dispose();
                _mutex = null;
            }   
        }
    }
}
