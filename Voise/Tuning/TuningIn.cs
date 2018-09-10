using Voise.TCP.Request;

namespace Voise.Tuning
{
    internal class TuningIn : Base
    {
        internal TuningIn(string path, InputMethod inputMethod, VoiseConfig config)
            : base(path, "in", inputMethod, config)
        {
        }

        internal override void SetResult(VoiseResult result)
        {
            _data.Add($"Transcript", result.Transcript);
            _data.Add($"Confidence", result.Confidence.ToString());

            _data.Add($"Intent", result.Intent);
            _data.Add($"Probability", result.Probability.ToString());
        }
    }
}
