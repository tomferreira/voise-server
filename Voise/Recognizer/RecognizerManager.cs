using System.Collections.Generic;
using Voise.Recognizer.Provider.Azure;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Provider.Google;
using Voise.Recognizer.Provider.Microsoft;

namespace Voise.Recognizer
{
    internal class RecognizerManager
    {
        private Dictionary<string, CommonRecognizer> _recognizers;

        // Microsoft is the default engine for recognizer.
        private const string DEFAULT_ENGINE_IDENTIFIER = MicrosoftRecognizer.ENGINE_IDENTIFIER;

        internal RecognizerManager(Config config)
        {
            List<string> recognizersEnabled = config.RecognizersEnabled;

            if (recognizersEnabled.Count == 0)
                throw new System.Exception("At least one recogning engine must be enabled.");

            _recognizers = new Dictionary<string, CommonRecognizer>();

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

            if (config.TuningEnabled)
            {
                foreach (CommonRecognizer recognizer in _recognizers.Values)
                    recognizer.EnableTuning(config.TuningPath);
            }
        }

        internal CommonRecognizer GetRecognizer(string engineID)
        {
            string finalEngineID = (engineID != null) ? engineID : DEFAULT_ENGINE_IDENTIFIER;

            if (!_recognizers.ContainsKey(finalEngineID.ToLower()))
                throw new System.Exception($"Recogning engine '{finalEngineID}' disabled or invalid.");

            return _recognizers[finalEngineID.ToLower()];
        }
    }
}
