using System.Threading.Tasks;
using Voise.General.Interface;
using Voise.Synthesizer.Provider.Common.Job;

namespace Voise.Synthesizer.Provider.Common
{
    internal abstract class CommonSynthesizer : ICommonSynthesizer
    {
        public IJob SetSynth(IAudioStream streamOut, string encoding, int sampleRate, string languageCode)
        {
            return CreateJob(streamOut, encoding, sampleRate, languageCode);
        }

        public async Task SynthAsync(IJob job, string text)
        {
            await job.SynthAsync(text).ConfigureAwait(false);
        }

        protected abstract IJob CreateJob(IAudioStream streamOut, string encoding, int sampleRate, string languageCode);

        public abstract int GetBytesPerSample(string encoding);
    }
}
