using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Voise
{
    internal class Pipeline
    {
        private Dictionary<Task, Func<Task>> _taskActions;
        private readonly SemaphoreSlim _mutex;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _cancellationToken;

        internal SpeechResult SpeechResult { get; private set; }

        // Internal async error
        internal Exception AsyncStreamError { get; set; }

        internal Pipeline()
        {
            _taskActions = new Dictionary<Task, Func<Task>>();
            _mutex = new SemaphoreSlim(0);

            _tokenSource = new CancellationTokenSource();
            _cancellationToken = _tokenSource.Token;

            SpeechResult = new SpeechResult();
        }

        internal Task StartNew(Func<Task> action)
        {
            var fakeTask = new Task(() => { }, _cancellationToken);
            _taskActions.Add(fakeTask, action);

            return fakeTask;
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

        internal async void WaitAll(params Task[] tasks)
        {
            try
            {
                foreach (var task in tasks)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        _cancellationToken.ThrowIfCancellationRequested();

                    Func<Task> action = _taskActions[task];

                    if (action != null)
                    {
                        await action();
                        _taskActions[task] = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
            finally
            {
                _taskActions.Clear();

                _tokenSource.Dispose();                
                _mutex.Dispose();
            }   
        }
    }
}
