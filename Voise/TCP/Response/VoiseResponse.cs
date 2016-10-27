namespace Voise.TCP.Response
{
    internal class VoiseResponse
    {
        internal class VoiseResult
        {
            public int code;
            public string message;
        }

        internal VoiseResponse(int code, string message)
        {
            result = new VoiseResult();
            result.code = code;
            result.message = message;
        }

        public VoiseResult result;
        public string utterance;
        public float confidence;
        public string intent;
        public float probability;
    };
}
