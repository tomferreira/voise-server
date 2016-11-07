using System.Collections.Generic;

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

            _streamingJobs.Add(streamOut, job);
        }

        internal void Synth(AudioStream streamOut, string text)
        {
            if (!_streamingJobs.ContainsKey(streamOut))
                throw new System.Exception("Job not exists.");

            Job job = _streamingJobs[streamOut];

            job.Synth(text);
        }

        internal static AudioEncoding ConvertAudioEncoding(string encoding)
        {
            switch (encoding.ToLower())
            {
                case "alaw":
                    return AudioEncoding.Alaw;

                case "linear16":
                    return AudioEncoding.Linear16;

                case "mulaw":
                    return AudioEncoding.Mulaw;

                default:
                    return AudioEncoding.EncodingUnspecified;
            }
        }
    }
}
