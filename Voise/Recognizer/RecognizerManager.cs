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

        internal Base GetRecognizer(string engineID)
        {
            // Microsoft is the default engine for recognizer.
            if (engineID == null)
                engineID = MicrosoftRecognizer.ENGINE_IDENTIFIER;

            switch (engineID.ToLower())
            {
                case MicrosoftRecognizer.ENGINE_IDENTIFIER:
                    return _microsoftRecognizer;

                case GoogleRecognizer.ENGINE_IDENTIFIER:
                    return _googleRecognizer;

                default:
                    throw new System.Exception($"Engine '{engineID}' not found.");
            }
        }
    }
}
