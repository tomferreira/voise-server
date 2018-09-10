using System.Collections.Generic;
using Voise.TCP.Request;

namespace Voise.Tuning
{
    internal class TuningIn : Base
    {
        internal TuningIn(string path, InputMethod inputMethod, VoiseConfig config)
            : base(path, "in", inputMethod, config)
        {
            _attrs.Add("Model Name", config.model_name);
            _attrs.Add("Context", string.Join(",", values: config.context ?? new List<string>()));
        }

        internal override void SetResult(VoiseResult result)
        {
            _attrs.Add("Transcript", result.Transcript);
            _attrs.Add("Confidence", result.Confidence.ToString());

            _attrs.Add("Intent", result.Intent);
            _attrs.Add("Probability", result.Probability.ToString());

            _shouldPersist = true;
        }
    }
}
