using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Cloud.Speech.V1Beta1;
using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Voise.Google.Cloud.Speech.V1Beta1
{
    /// <summary>
    /// Wrapper around <see cref="SpeechClient"/>, providing more idiomatic .NET names and better streaming support.
    /// </summary>
    public class SpeechRecognizer
    {
        private readonly SpeechClient _client;

        // Note: eventually SpeechClient will have an OperationsClient itself, at which point the second parameter
        // can be removed.
        public SpeechRecognizer(SpeechClient client)
        {
            _client = client;
        }

        public static SpeechRecognizer Create(ServiceEndpoint endpoint = null)
        {
            // TODO: Use a single channel... should be fine when SpeechClient has an OperationsClient.
            return new SpeechRecognizer(SpeechClient.Create(endpoint));
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

        public async Task<RecognizerStream> BeginStreamingRecognizeAsync(StreamingRecognitionConfig config, CallSettings settings = null)
        {
            var stream = _client.GrpcClient.StreamingRecognize(new CallOptions());
            await stream.RequestStream.WriteAsync(new StreamingRecognizeRequest { StreamingConfig = config }).ConfigureAwait(false);
            return new RecognizerStream(config, stream);
        }
    }
}
