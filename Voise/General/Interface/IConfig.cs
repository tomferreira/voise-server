using System.Collections.Generic;

namespace Voise.General.Interface
{
    public interface IConfig
    {
        int Port { get; }

        List<string> RecognizersEnabled { get; }

        List<string> SynthesizersEnabled { get; }

        string ClassifiersPath { get; }

        bool TuningEnabled { get; }

        string TuningPath { get; }

        int TuningRetentionDays { get; }

        string GetRecognizerAttribute(params string[] identifiers);

        string GetSynthesizerAttribute(params string[] identifiers);

        string GetTuningAttribute(params string[] identifiers);
    }
}
