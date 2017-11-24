using System.Collections.Generic;
using Voise.Recognizer.Azure;
using Voise.Recognizer.Google;
using Voise.Recognizer.Microsoft;

namespace Voise.Recognizer
{
    internal class RecognizerManager
    {
        private Dictionary<string, Base> _recognizers;

        internal RecognizerManager(Config config)
        {
            List<string> recognizersEnabled = config.RecognizersEnabled;

            if (recognizersEnabled.Count == 0)
                throw new System.Exception("At least one recogning engine must be enabled.");

            _recognizers = new Dictionary<string, Base>();

            if (recognizersEnabled.Contains(MicrosoftRecognizer.ENGINE_IDENTIFIER))
                _recognizers.Add(MicrosoftRecognizer.ENGINE_IDENTIFIER, new MicrosoftRecognizer());

            if (recognizersEnabled.Contains(GoogleRecognizer.ENGINE_IDENTIFIER))
            {
                string credentialPath = config.GetRecognizerAttribute("google", "credential_path");

                _recognizers.Add(GoogleRecognizer.ENGINE_IDENTIFIER, new GoogleRecognizer(credentialPath));
            }

            if (recognizersEnabled.Contains(AzureRecognizer.ENGINE_IDENTIFIER))
            {
                string primaryKey = config.GetRecognizerAttribute("azure", "primarykey");

                _recognizers.Add(AzureRecognizer.ENGINE_IDENTIFIER, new AzureRecognizer(primaryKey));
            }
        }

        internal Base GetRecognizer(string engineID)
        {
            // Microsoft is the default engine for recognizer.
            if (engineID == null)
                engineID = MicrosoftRecognizer.ENGINE_IDENTIFIER;

            if (!_recognizers.ContainsKey(engineID.ToLower()))
                throw new System.Exception($"Recogning engine '{engineID}' disabled or invalid.");

            return _recognizers[engineID.ToLower()];
        }
    }
}
