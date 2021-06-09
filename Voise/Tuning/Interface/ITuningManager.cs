using Voise.TCP.Request;

namespace Voise.Tuning.Interface
{
    public interface ITuningManager
    {
        TuningIn CreateTuningIn(Base.InputMethod inputMethod, VoiseConfig config);

        TuningOut CreateTuningOut(Base.InputMethod inputMethod, string text, VoiseConfig config);
    }
}
