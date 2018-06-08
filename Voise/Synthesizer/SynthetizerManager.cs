
using System.Collections.Generic;
using Voise.Synthesizer.Provider.Common;
using Voise.Synthesizer.Provider.Microsoft;

namespace Voise.Synthesizer
{
    internal class SynthetizerManager
    {
        private Dictionary<string, CommonSynthetizer> _synthetizers;

        // Microsoft is the default engine for recognizer.
        private const string DEFAULT_ENGINE_IDENTIFIER = MicrosoftSynthetizer.ENGINE_IDENTIFIER;

        internal SynthetizerManager(Config config)
        {
            List<string> synthetizersEnabled = config.SynthetizersEnabled;

            if (synthetizersEnabled.Count == 0)
                throw new System.Exception("At least one synthetizer engine must be enabled.");

            _synthetizers = new Dictionary<string, CommonSynthetizer>();

            if (synthetizersEnabled.Contains(MicrosoftSynthetizer.ENGINE_IDENTIFIER))
                _synthetizers.Add(MicrosoftSynthetizer.ENGINE_IDENTIFIER, new MicrosoftSynthetizer());
        }

        internal CommonSynthetizer GetSynthetizer(string engineID)
        {
            string finalEngineID = (engineID != null) ? engineID : DEFAULT_ENGINE_IDENTIFIER;

            if (!_synthetizers.ContainsKey(finalEngineID.ToLower()))
                throw new System.Exception($"Synthetizer engine '{finalEngineID}' disabled or invalid.");

            return _synthetizers[finalEngineID.ToLower()];
        }
    }
}
