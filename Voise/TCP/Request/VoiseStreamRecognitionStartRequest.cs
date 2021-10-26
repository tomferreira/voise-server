using Newtonsoft.Json;

namespace Voise.TCP.Request
{
    internal class VoiseStreamRecognitionStartRequest
    {
        [JsonProperty]
        public VoiseConfig Config { get; private set; }
    };
}
