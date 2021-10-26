using Newtonsoft.Json;

namespace Voise.TCP.Request
{
    internal class VoiseSyncRecognitionRequest
    {
        [JsonProperty]
        public VoiseConfig Config { get; private set; }

        // Raw audio bytes (base64 encoded)
        [JsonProperty("audio")]
        public string Audio { get; private set; }
    };
}
