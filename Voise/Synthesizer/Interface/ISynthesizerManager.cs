using Voise.Synthesizer.Provider.Common;

namespace Voise.Synthesizer.Interface
{
    public interface ISynthesizerManager
    {
        ICommonSynthesizer GetSynthesizer(string engineID);
    }
}
