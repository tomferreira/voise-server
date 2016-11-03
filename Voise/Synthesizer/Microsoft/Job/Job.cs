using System;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using Voise.Synthesizer.Exception;

namespace Voise.Synthesizer.Microsoft
{
    internal class Job
    {
        private SpeechSynthesizer _speechSynthesizer;
        private SpeechAudioFormatInfo _info;
        private AudioStream _streamOut;
        private string _languageCode;

        internal Job(AudioStream streamOut, SynthetizerVoice voice, AudioEncoding encoding, int sampleRate, string languageCode)
        {
            _speechSynthesizer = new SpeechSynthesizer();

            ValidateArguments(encoding, sampleRate, languageCode);

            _streamOut = streamOut;
            _languageCode = languageCode;

            _info = new SpeechAudioFormatInfo(
                encoding.Format, sampleRate, encoding.BitsPerSample, encoding.ChannelCount, sampleRate, 1, null);

            try
            {
                _speechSynthesizer.SelectVoice(voice.Name);
            }
            catch(ArgumentException e)
            {
                throw new BadVoiceException(e.Message);
            }
        }

        internal void Synth(string text)
        {
            _streamOut.Start();

            WaveStream waveStream = new WaveStream();

            _speechSynthesizer.SetOutputToAudioStream(waveStream, _info);
            //_speechSynthesizer.SetOutputToWaveStream(waveStream);

            string script = string.Format(
                "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"{0}\">{1}</speak>", _languageCode, text);

            Prompt prompt = new Prompt(script, SynthesisTextFormat.Ssml);

            waveStream.Progress += WaveStream_Progress;

            _speechSynthesizer.Speak(prompt);
            _speechSynthesizer.SetOutputToNull();

            _streamOut.Stop();
        }

        private void WaveStream_Progress(object sender, WaveStream.ProgressEventArgs e)
        {
            _streamOut.Write(e.Chunk);
        }

        private void ValidateArguments(AudioEncoding encoding, int sampleRate, string languageCode)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");

            ValidateLanguage(languageCode);
        }

        private void ValidateLanguage(string languageCode)
        {
            bool found = false;

            foreach (InstalledVoice voice in _speechSynthesizer.GetInstalledVoices())
            {
                if (voice.VoiceInfo.Culture.Name == languageCode)
                    found = true;
            }

            if (!found)
                throw new BadLanguageException($"Language '{languageCode}' not found.");
        }
    }
}
