namespace Voise.TCP.Response
{
    internal class VoiseResponse
    {
        internal class VoiseResult
        {
            public int code;
            public string message;
        }

        internal class VoiseAudio
        {
            public string content;  // Base64 encoded
            public long length;
        }

        internal VoiseResponse(ResponseCode code, string message = null)
        {
            result = new VoiseResult
            {
                code = code.Code,
                message = message ?? code.Message
            };
        }

        public VoiseResult result;

        // ASR
        public string utterance;
        public float confidence;
        public string intent;
        public float probability;

        // TTS
        public VoiseAudio audio;
    };
}
