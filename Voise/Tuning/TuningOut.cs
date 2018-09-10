
using Voise.TCP.Request;

namespace Voise.Tuning
{
    internal class TuningOut : Base
    {
        internal TuningOut(string path, InputMethod inputMethod, string text, VoiseConfig config)
            : base(path, "out", inputMethod, config)
        {
            _attrs.Add("Text", text);
        }

        internal override void SetResult(VoiseResult result)
        {
            _shouldPersist = true;
        }
    }
}
