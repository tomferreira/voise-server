using System.Threading.Tasks;
using Voise.General.Interface;
using Voise.Synthesizer.Provider.Common.Job;

namespace Voise.Synthesizer.Provider.Common
{
    public interface ICommonSynthesizer
    {
        IJob SetSynth(IAudioStream streamOut, string encoding, int sampleRate, string languageCode);

        Task SynthAsync(IJob job, string text);

        int GetBytesPerSample(string encoding);
    }
}
