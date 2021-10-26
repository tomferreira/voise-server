using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Synthesis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Voise.General.Interface;
using Voise.Provider.Microsoft;
using Voise.Synthesizer.Exception;
using Voise.Synthesizer.Provider.Common.Job;

namespace Voise.Synthesizer.Provider.Microsoft
{
    internal class Job : IJob
    {
        private SpeechSynthesizer _speechSynthesizer;
        private readonly SpeechAudioFormatInfo _info;
        private readonly IAudioStream _streamOut;

        internal Job(IAudioStream streamOut, AudioEncoding encoding, int sampleRate, string languageCode)
        {
            _speechSynthesizer = new SpeechSynthesizer();

            ValidateArguments(encoding, sampleRate);

            _streamOut = streamOut;

            _info = new SpeechAudioFormatInfo(
                encoding.Format, sampleRate, encoding.BitsPerSample,
                encoding.ChannelCount, sampleRate * encoding.BitsPerSample / 8, encoding.BlockAlign, null);

            InstalledVoice voice = GetInstalledVoice(languageCode, false);

            if (voice == null || !voice.Enabled)
                throw new BadVoiceException($"Voice not found for language '{languageCode}'");

            _speechSynthesizer.SelectVoice(voice.VoiceInfo.Name);
        }

        public async Task SynthAsync(string text)
        {
            await Task.Run(() =>
            {
                using (WaveStream waveStream = new WaveStream())
                {
                    _streamOut.Start();

                    _speechSynthesizer.SetOutputToAudioStream(waveStream, _info);

                    string script = string.Format(
                        "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"{0}\">{1}</speak>",
                        _speechSynthesizer.Voice.Culture.Name, text);

                    Prompt prompt = new Prompt(script, SynthesisTextFormat.Ssml);

                    waveStream.Progress += WaveStream_Progress;

                    _speechSynthesizer.Speak(prompt);
                    _speechSynthesizer.SetOutputToNull();

                    _streamOut.Stop();
                }
            }).ConfigureAwait(false);
        }

        private void WaveStream_Progress(object sender, WaveStream.ProgressEventArgs e)
        {
            _streamOut.Write(e.Chunk);
        }

        private static void ValidateArguments(AudioEncoding encoding, int sampleRate)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");
        }

        private InstalledVoice GetInstalledVoice(string languageCode, bool strict)
        {
            if (languageCode == null)
                return null;

            var installedVoices = _speechSynthesizer.GetInstalledVoices();

            var voiceSelected = installedVoices.FirstOrDefault(
                    voice => string.Equals(voice.VoiceInfo.Culture.Name, languageCode, StringComparison.OrdinalIgnoreCase));

            if (strict || voiceSelected != null)
                return voiceSelected;

            string prefixLanguageCode = languageCode.Split('-').First();

            return installedVoices.FirstOrDefault(
                voice => string.Equals(voice.VoiceInfo.Culture.Name.Split('-').First(), prefixLanguageCode, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _speechSynthesizer.Dispose();
                _speechSynthesizer = null;
            }
        }
    }
}
