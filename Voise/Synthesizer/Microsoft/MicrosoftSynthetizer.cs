using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voise.Synthesizer.Microsoft
{
    internal class MicrosoftSynthetizer
    {
        private Dictionary<AudioStream, Job> _streamingJobs;

        internal MicrosoftSynthetizer()
        {
            _streamingJobs = new Dictionary<AudioStream, Job>();
        }

        internal void Create(AudioStream streamOut, AudioEncoding encoding, int sampleRate, string languageCode)
        {
            Job job = new Job(streamOut, encoding, sampleRate, languageCode);

            lock (_streamingJobs)
                _streamingJobs.Add(streamOut, job);
        }

        internal async Task SynthAsync(AudioStream streamOut, string text)
        {
            Job job = null;

            lock (_streamingJobs)
            {
                if (!_streamingJobs.ContainsKey(streamOut))
                    throw new System.Exception("Job not exists.");

                job = _streamingJobs[streamOut];
            }

            await job.SynthAsync(text);

            lock (_streamingJobs)
                _streamingJobs.Remove(streamOut);
        }

        internal static AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case "flac":
                    throw new System.Exception("Codec 'flac' not supported.");

                case "linear16":
                    return AudioEncoding.Linear16;

                case "alaw":
                    return AudioEncoding.Alaw;

                case "mulaw":
                    return AudioEncoding.Mulaw;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}
