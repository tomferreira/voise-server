﻿using Google.Api.Gax;
using Google.Cloud.Speech.V1Beta1;
using Google.Longrunning;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;
using Voise.Google.Longrunning;

namespace Voise.Google.Cloud.Speech.V1Beta1
{
    /// <summary>
    /// Wrapper around <see cref="SpeechClient"/>, providing more idiomatic .NET names and better streaming support.
    /// </summary>
    public class SpeechRecognizer
    {
        private readonly SpeechClient _client;
        private readonly OperationsClient _operationsClient;

        // Note: eventually SpeechClient will have an OperationsClient itself, at which point the second parameter
        // can be removed.
        public SpeechRecognizer(SpeechClient client, OperationsClient operationsClient)
        {
            _client = client;
            _operationsClient = operationsClient;
        }

        public static SpeechRecognizer Create()
        {
            // TODO: Use a single channel... should be fine when SpeechClient has an OperationsClient.
            return new SpeechRecognizer(SpeechClient.Create(), OperationsClient.Create(SpeechClient.DefaultEndpoint));
        }

        public SyncRecognizeResponse Recognize(RecognitionConfig config, RecognitionAudio audio, CallSettings callSettings = null)
            => _client.SyncRecognize(
                GaxPreconditions.CheckNotNull(config, nameof(config)),
                GaxPreconditions.CheckNotNull(audio, nameof(audio)),
                callSettings);

        public Task<SyncRecognizeResponse> RecognizeAsync(RecognitionConfig config, RecognitionAudio audio, CallSettings callSettings = null)
            => _client.SyncRecognizeAsync(
                GaxPreconditions.CheckNotNull(config, nameof(config)),
                GaxPreconditions.CheckNotNull(audio, nameof(audio)),
                callSettings);

        public Task<SyncRecognizeResponse> RecognizeAsync(RecognitionConfig config, RecognitionAudio audio, CancellationToken cancellationToken)
            => _client.SyncRecognizeAsync(
                GaxPreconditions.CheckNotNull(config, nameof(config)),
                GaxPreconditions.CheckNotNull(audio, nameof(audio)),
                cancellationToken);

        public Operation<AsyncRecognizeResponse> BeginRecognize(RecognitionConfig config, RecognitionAudio audio, CallSettings callSettings = null)
        {
            var operation = _client.AsyncRecognize(
                GaxPreconditions.CheckNotNull(config, nameof(config)),
                GaxPreconditions.CheckNotNull(audio, nameof(audio)),
                callSettings);
            return new Operation<AsyncRecognizeResponse>(operation, _operationsClient);
        }

        public Task<Operation<AsyncRecognizeResponse>> BeginRecognizeAsync(RecognitionConfig config, RecognitionAudio audio, CancellationToken cancellationToken)
            => BeginRecognizeAsync(config, audio, new CallSettings { CancellationToken = cancellationToken });

        public async Task<Operation<AsyncRecognizeResponse>> BeginRecognizeAsync(RecognitionConfig config, RecognitionAudio audio, CallSettings callSettings = null)
        {
            var operation = await _client.AsyncRecognizeAsync(
                GaxPreconditions.CheckNotNull(config, nameof(config)),
                GaxPreconditions.CheckNotNull(audio, nameof(audio)),
                callSettings).ConfigureAwait(false);
            return new Operation<AsyncRecognizeResponse>(operation, _operationsClient);
        }

        // TODO: CallSettings?
        public Operation<AsyncRecognizeResponse> PollRecognize(string operationName) =>
            Operation<AsyncRecognizeResponse>.PollFromName(operationName, _operationsClient);

        public Task<Operation<AsyncRecognizeResponse>> PollRecognizeAsync(string operationName) =>
            Operation<AsyncRecognizeResponse>.PollFromNameAsync(operationName, _operationsClient);

        public async Task<RecognizerStream> BeginStreamingRecognizeAsync(StreamingRecognitionConfig config, CallSettings settings = null)
        {
            var stream = _client.GrpcClient.StreamingRecognize(new CallOptions());
            await stream.RequestStream.WriteAsync(new StreamingRecognizeRequest { StreamingConfig = config }).ConfigureAwait(false);
            return new RecognizerStream(config, stream);
        }
    }
}
