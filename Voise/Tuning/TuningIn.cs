using System.Collections.Generic;
using Voise.General;
using Voise.TCP.Request;

namespace Voise.Tuning
{
    public class TuningIn : Base
    {
        public TuningIn(string path, InputMethod inputMethod, VoiseConfig config)
            : base(path, "in", inputMethod, config)
        {
            _attrs.Add("Model Name", config.ModelName);
            _attrs.Add("Context", string.Join(",", values: config.Context ?? new List<string>()));
        }

        public override void SetResult(VoiseResult result)
        {
            _attrs.Add("Transcript", result.Transcript);
            _attrs.Add("Confidence", result.Confidence.ToString());

            _attrs.Add("Intent", result.Intent);
            _attrs.Add("Probability", result.Probability.ToString());

            _shouldPersist = true;
        }
    }
}
