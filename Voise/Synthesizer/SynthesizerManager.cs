
using log4net;
using System.Collections.Generic;
using Voise.General.Interface;
using Voise.Synthesizer.Interface;
using Voise.Synthesizer.Provider.Common;
using Voise.Synthesizer.Provider.Microsoft;

namespace Voise.Synthesizer
{
    public class SynthesizerManager : ISynthesizerManager
    {
        private readonly Dictionary<string, ICommonSynthesizer> _synthesizers;

        // Microsoft is the default engine for recognizer.
        private const string DEFAULT_ENGINE_IDENTIFIER = MicrosoftSynthesizer.ENGINE_IDENTIFIER;

        public SynthesizerManager(IConfig config, ILog logger)
        {
            List<string> synthesizersEnabled = config.SynthesizersEnabled;

            if (synthesizersEnabled.Count == 0)
                throw new System.Exception("At least one synthesizer engine must be enabled.");

            _synthesizers = new Dictionary<string, ICommonSynthesizer>();

            if (synthesizersEnabled.Contains(MicrosoftSynthesizer.ENGINE_IDENTIFIER))
                _synthesizers.Add(MicrosoftSynthesizer.ENGINE_IDENTIFIER, new MicrosoftSynthesizer());
        }

        public ICommonSynthesizer GetSynthesizer(string engineID)
        {
            string finalEngineID = engineID ?? DEFAULT_ENGINE_IDENTIFIER;

            if (!_synthesizers.ContainsKey(finalEngineID.ToLowerInvariant()))
                throw new System.Exception($"Synthesizer engine '{finalEngineID}' disabled or invalid.");

            return _synthesizers[finalEngineID.ToLowerInvariant()];
        }
    }
}
