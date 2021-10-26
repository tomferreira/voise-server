using Newtonsoft.Json;

namespace Voise.TCP.Request
{
    class VoiseSynthVoiceRequest
    {
        [JsonProperty]
        public VoiseConfig Config { get; private set; }

        [JsonProperty("text")]
        public string Text { get; private set; }
    }
}
