using System.Collections.Generic;
using System.IO;
using Voise.Recognizer;

namespace Voise.Tuning
{
    internal class TuningIn : Base
    {
        internal TuningIn(string path, InputMethod inputMethod, string encoding, int sampleRate, string languageCode)
            : base(path, "in", inputMethod, encoding, sampleRate, languageCode)
        {
        }

        internal void SaveSpeechRecognitionResult(SpeechRecognitionResult result)
        {
            List<string> contents = new List<string>();

            contents.Add($"Input Method: {_inputMethod.ToString()}");
            contents.Add($"Transcript: {result.Transcript}");
            contents.Add($"Confidence: {result.Confidence}");

            File.WriteAllLines(_resultPath, contents);
        }
    }
}
