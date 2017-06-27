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

    internal class VoiseConfig
    {
#pragma warning disable 0649
        public string engine_id;
        public string encoding;
        public int sample_rate;
        public string language_code;
        public string model_name;
        public List<string> context;
#pragma warning restore 0649
    }
}
