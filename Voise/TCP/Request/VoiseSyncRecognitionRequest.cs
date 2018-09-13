namespace Voise.TCP.Request
{
    internal class VoiseSyncRecognitionRequest
    {
#pragma warning disable 0649
        public VoiseConfig Config;
        public string audio;  // Raw audio bytes (base64 encoded)
#pragma warning restore 0649
    };
}
