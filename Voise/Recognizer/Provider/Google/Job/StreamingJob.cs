using Google.Cloud.Speech.V1;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voise.General;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Google.Internal;
using static Google.Cloud.Speech.V1.RecognitionConfig.Types;

namespace Voise.Recognizer.Provider.Google.Job
{
    internal class StreamingJob : Base, IStreamingJob
    {
        private StreamingRecognitionConfig _config;
        private RecognizerStream _recognizerStream;
        private RequestQueue<ByteString> _requestQueue;
        private Task _doneTask;

        private AudioStream _streamIn;

        internal StreamingJob(SpeechRecognizer recognizer, AudioStream streamIn, AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
            : base(recognizer)
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _recognizer = recognizer;

            _config = new StreamingRecognitionConfig
            {
                Config = new RecognitionConfig
                {
                    Encoding = encoding,
                    SampleRateHertz = sampleRate,
                    MaxAlternatives = 5,
                    LanguageCode = languageCode,
                    SpeechContexts = { CreateSpeechContext(contexts) }
                },
                InterimResults = true
            };

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
            _streamIn.StreamingStopped += StreamingStopped;
        }

        public async Task StartAsync()
        {
            _recognizerStream =
                await _recognizer.BeginStreamingRecognizeAsync(_config).ConfigureAwait(false);

            _requestQueue = new RequestQueue<ByteString>(_recognizerStream.RequestStream, 100);

            _streamIn.Start();
            _doneTask = ConsumeResultsAsync();
        }

        private async void StreamingStopped(object sender, EventArgs e)
        {
            await _requestQueue.CompleteAsync().ConfigureAwait(false);
        }

        private void ConsumeStreamData(object sender, AudioStream.StreamInEventArgs e)
        {
            ByteString data = ByteString.CopyFrom(e.Buffer, 0, e.BytesStreamed);
            _requestQueue.Post(data);
        }

        private async Task ConsumeResultsAsync()
        {
            var cancellationToken = new System.Threading.CancellationToken();
            var responses = _recognizerStream.ResponseStream;

            while (await responses.MoveNext(cancellationToken).ConfigureAwait(false))
            {
                var response = responses.Current;

                if (response.Results.Any())
                {
                    foreach (var result in response.Results)
                    {
                        if (result.IsFinal)
                        {
                            foreach (var alternative in result.Alternatives)
                            {
                                if (BestAlternative == SpeechRecognitionResult.NoResult || BestAlternative.Confidence < alternative.Confidence)
                                    BestAlternative = new SpeechRecognitionResult(alternative.Transcript, alternative.Confidence);
                            }
                        }
                    }
                }
            }
        }

        public async Task StopAsync()
        {
            _streamIn.Stop();

            // This will complete when the gRPC stream has completed.
            await _doneTask;
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _recognizerStream.Dispose();
                _doneTask?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
