using Google.Cloud.Speech.V1Beta1;

namespace Voise.Recognizer
{
    internal class NoResultSpeechRecognitionAlternative
    {
        internal static readonly SpeechRecognitionAlternative Default = new SpeechRecognitionAlternative();

        static NoResultSpeechRecognitionAlternative()
        {
            Default.Transcript = "#NORESULT";
            Default.Confidence = -1;
        }
    }
}
