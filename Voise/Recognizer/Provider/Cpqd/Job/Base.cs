using CPqDAsr.ASR;
using CPqDASR;
using CPqDASR.ASR;
using CPqDASR.Config;
using CPqDASR.Entities;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Voise.Recognizer.Exception;

namespace Voise.Recognizer.Provider.Cpqd.Job
{
    internal class Base : IDisposable
    {
        protected readonly SpeechRecognizer _speechRecognizer;
        protected LanguageModelList _modelList;
        protected Internal.BufferAudioSource _audioSource;

        protected object _monitorCompleted;
        protected bool _completed;

        protected ILog _log;
        private bool _disposed;

        public SpeechRecognitionResult BestAlternative { get; private set; }

        internal Base(ClientConfig config, LanguageModelList modelList)
        {
            _monitorCompleted = new object();
            _completed = false;

            _speechRecognizer = SpeechRecognizer.Create(config);
            _speechRecognizer.OnRecognitionResult += OnRecognitionResult;
            _speechRecognizer.OnError += OnError;

            _modelList = modelList;

            // Set as default alternative
            BestAlternative = SpeechRecognitionResult.NoResult;
        }

        private void OnError()
        {
            _log.Error("There was occurred a unknowed error.");
        }

        private void OnRecognitionResult(RecognitionResult result)
        {
            if (result.ResultCode.HasValue && result.ResultCode.Value == RecognitionResultCode.RECOGNIZED)
            {
                RecognitionAlternative bestResult =
                    result.Alternatives.OrderByDescending(x => x.Confidence).First();

                BestAlternative = new SpeechRecognitionResult(
                    bestResult.Text, (float)bestResult.Confidence / 100);
            }

            lock (_monitorCompleted)
            {
                _completed = true;
                Monitor.Pulse(_monitorCompleted);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _speechRecognizer?.Dispose();

            _disposed = true;   
        }
    }

}