
using System.Collections.Generic;
using Voise.General;
using Voise.Synthesizer.Provider.Common;
using Voise.Synthesizer.Provider.Microsoft;

namespace Voise.Synthesizer
{
    internal class SynthesizerManager
    {
        private Dictionary<string, CommonSynthesizer> _synthesizers;

        // Microsoft is the default engine for recognizer.
        private const string DEFAULT_ENGINE_IDENTIFIER = MicrosoftSynthesizer.ENGINE_IDENTIFIER;

        internal SynthesizerManager(Config config)
        {
            List<string> synthesizersEnabled = config.SynthesizersEnabled;

            if (synthesizersEnabled.Count == 0)
                throw new System.Exception("At least one synthesizer engine must be enabled.");

            _synthesizers = new Dictionary<string, CommonSynthesizer>();

            if (synthesizersEnabled.Contains(MicrosoftSynthesizer.ENGINE_IDENTIFIER))
                _synthesizers.Add(MicrosoftSynthesizer.ENGINE_IDENTIFIER, new MicrosoftSynthesizer());
        }

        internal CommonSynthesizer GetSynthesizer(string engineID)
        {
            string finalEngineID = engineID ?? DEFAULT_ENGINE_IDENTIFIER;

            if (!_synthesizers.ContainsKey(finalEngineID.ToLower()))
                throw new System.Exception($"Synthesizer engine '{finalEngineID}' disabled or invalid.");

            return _synthesizers[finalEngineID.ToLower()];
        }
    }
}
