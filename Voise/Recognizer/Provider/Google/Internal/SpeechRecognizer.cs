using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using Grpc.Core;
using System.IO;
using System.Threading.Tasks;

namespace Voise.Recognizer.Provider.Google.Internal
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

        public static SpeechRecognizer Create(string credentialPath)
        {
            if (!File.Exists(credentialPath))
                throw new System.Exception($"Credential path '{credentialPath}' not found.");

            var fileStream = new FileStream(credentialPath, FileMode.Open);
            GoogleCredential googleCredential = GoogleCredential.FromStream(fileStream);
            ChannelCredentials channelCredentials = GoogleGrpcCredentials.ToChannelCredentials(googleCredential);
            Channel channel = new Channel("speech.googleapis.com", channelCredentials);

            // TODO: Use a single channel... should be fine when SpeechClient has an OperationsClient.
            return new SpeechRecognizer(SpeechClient.Create(channel));
        }

        public RecognizeResponse Recognize(RecognitionConfig config, RecognitionAudio audio, CallSettings callSettings = null)
            => _client.Recognize(
                GaxPreconditions.CheckNotNull(config, nameof(config)),
                GaxPreconditions.CheckNotNull(audio, nameof(audio)),
                callSettings);

        public Task<RecognizeResponse> RecognizeAsync(RecognitionConfig config, RecognitionAudio audio, CallSettings callSettings = null)
            => _client.RecognizeAsync(
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
