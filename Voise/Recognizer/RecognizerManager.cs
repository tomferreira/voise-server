using Voise.Recognizer.Google;
using Voise.Recognizer.Microsoft;

namespace Voise.Recognizer
{
    internal class RecognizerManager
    {
        private GoogleRecognizer _googleRecognizer;
        private MicrosoftRecognizer _microsoftRecognizer;

        internal RecognizerManager()
        {
            _googleRecognizer = new GoogleRecognizer();
            _microsoftRecognizer = new MicrosoftRecognizer();
        }

        internal Base GetRecognizer(string engine)
        {
            // Microsoft is the default engine for recognizer.
            if (engine == null)
                engine = MicrosoftRecognizer.ENGINE_NAME;

            switch (engine.ToLower())
            {
                case MicrosoftRecognizer.ENGINE_NAME:
                    return _microsoftRecognizer;

                case GoogleRecognizer.ENGINE_NAME:
                    return _googleRecognizer;

                default:
                    throw new System.Exception($"Engine '{engine}' not found.");
            }
        }
    }
}
