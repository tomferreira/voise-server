using System.Collections.Generic;
using Voise.TCP.Request;

namespace Voise.Tuning
{
    internal class TuningIn : Base
    {
        internal TuningIn(string path, InputMethod inputMethod, VoiseConfig config)
            : base(path, "in", inputMethod, config)
        {
            _data.Add("Model Name", config.model_name);
            _data.Add("Context", string.Join(",", values: config.context ?? new List<string>()));
        }

        internal override void SetResult(VoiseResult result)
        {
            _data.Add($"Transcript", result.Transcript);
            _data.Add($"Confidence", result.Confidence.ToString());

            _data.Add($"Intent", result.Intent);
            _data.Add($"Probability", result.Probability.ToString());

            _shouldPersist = true;
        }
    }
}
