
using System.Globalization;
using Voise.General;
using Voise.TCP.Request;

namespace Voise.Tuning
{
    public class TuningOut : Base
    {
        public TuningOut(string path, InputMethod inputMethod, string text, VoiseConfig config)
            : base(path, "out", inputMethod, config)
        {
            _attrs.Add("Max frame (ms)", (config.max_frame_ms ?? 20).ToString(CultureInfo.InvariantCulture));
            _attrs.Add("Text", text);
        }

        public override void SetResult(VoiseResult result)
        {
            _shouldPersist = true;
        }
    }
}
