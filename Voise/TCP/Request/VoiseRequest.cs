using Newtonsoft.Json;
using System.Collections.Generic;

namespace Voise.TCP.Request
{
    internal class VoiseRequest
    {
        // ASR
        public VoiseSyncRecognitionRequest SyncRequest { get; set; }

        public VoiseStreamRecognitionStartRequest StreamStartRequest { get; set; }
        public VoiseStreamRecognitionDataRequest StreamDataRequest { get; set; }
        public VoiseStreamRecognitionStopRequest StreamStopRequest { get; set; }

        // TTS
        public VoiseSynthVoiceRequest SynthVoiceRequest { get; set; }
    };

    public class VoiseConfig
    {
        [JsonProperty("engine_id")]
        public string EngineID { get; private set; }

        [JsonProperty("encoding")]
        public string Encoding { get; private set; }

        [JsonProperty("sample_rate")]
        public int SampleRate { get; private set; }

        [JsonProperty("language_code")]
        public string LanguageCode { get; private set; }

        // Only for ASR
        [JsonProperty("model_name")]
        public string ModelName { get; private set; }

        [JsonProperty("context")]
        public List<string> Context { get; private set; }

        // Only for TTS
        [JsonProperty("max_frame_ms")]
        public int? MaxFrameMS { get; private set; }
    }
}
