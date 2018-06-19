namespace Voise.Recognizer
{
    internal class SpeechRecognitionResult
    {
        internal static readonly SpeechRecognitionResult NoResult
            = new SpeechRecognitionResult("#NORESULT", 1);

        public string Transcript { get; private set; }

        public float Confidence { get; private set; }

        internal SpeechRecognitionResult(string transcript, float confidence)
        {
            Transcript = transcript;
            Confidence = confidence;
        }
    }
}
