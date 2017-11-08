namespace Voise.TCP.Request
{
    internal class VoiseSyncRecognitionRequest
    {
#pragma warning disable 0649
        public VoiseConfig Config;
        public string audio; // Base64 encoded
#pragma warning restore 0649
    };
}
