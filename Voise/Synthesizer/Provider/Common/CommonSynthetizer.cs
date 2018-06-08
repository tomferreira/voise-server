using System.Threading.Tasks;
using Voise.Synthesizer.Provider.Common.Job;

namespace Voise.Synthesizer.Provider.Common
{
    internal abstract class CommonSynthetizer
    {
        internal IJob SetSynth(AudioStream streamOut, string encoding, int sampleRate, string languageCode)
        {
            return CreateJob(streamOut, encoding, sampleRate, languageCode);
        }

        internal async Task SynthAsync(IJob job, string text)
        {
            await job.SynthAsync(text).ConfigureAwait(false);
        }

        protected abstract IJob CreateJob(AudioStream streamOut, string encoding, int sampleRate, string languageCode);
    }
}
