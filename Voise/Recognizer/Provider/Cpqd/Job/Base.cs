using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CPqDAsr.ASR;
using CPqDASR;
using CPqDASR.ASR;
using CPqDASR.Config;
using CPqDASR.Entities;
using BufferAudioSource = Voise.Recognizer.Provider.Cpqd.Internal.BufferAudioSource;

namespace Voise.Recognizer.Provider.Cpqd.Job
{
    internal class Base : IDisposable
    {
        protected readonly SpeechRecognizer _speechRecognizer;
        protected LanguageModelList _modelList;
        protected BufferAudioSource _audioSource;

        protected object _monitorCompleted;
        protected bool _completed;
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
        }

        private void OnError()
        {
            throw new System.NotImplementedException();
        }

        private void OnRecognitionResult(RecognitionResult result)
        {
            GetResult(new List<RecognitionResult> { result });

            lock (_monitorCompleted)
            {
                _completed = true;
                Monitor.Pulse(_monitorCompleted);
            }
        }

        private void GetResult(List<RecognitionResult> recognitionResults)
        {
            if (!recognitionResults.Any(x => x.ResultCode.HasValue && x.ResultCode.Value == RecognitionResultCode.RECOGNIZED))
            {
                BestAlternative = SpeechRecognitionResult.NoResult;
                return;
            }

            BestAlternative = new SpeechRecognitionResult("", 0);

            foreach (var recognitionResult in recognitionResults.Where(x => x.ResultCode.HasValue && x.ResultCode.Value == RecognitionResultCode.RECOGNIZED))
            {
                foreach (var alternative in recognitionResult.Alternatives)
                {
                    if ((float)alternative.Confidence / 100 > BestAlternative.Confidence)
                    {
                        var speechRecognitionResult =
                            new SpeechRecognitionResult(recognitionResult.Alternatives[0].Text,
                                (float)recognitionResult.Alternatives[0].Confidence / 100);
                        BestAlternative = speechRecognitionResult;
                    }
                }
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