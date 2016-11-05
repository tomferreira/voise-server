using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Exception;
using Voise.Synthesizer.Microsoft;

namespace Voise.Recognizer.Microsoft.Job
{
    internal class SyncJob : Base
    {
        internal SyncJob(string audio_base64, AudioEncoding encoding, int sampleRate, string languageCode)
            : base()
        {
            _info = new SpeechAudioFormatInfo(encoding.Format, sampleRate, encoding.BitsPerSample,
                encoding.ChannelCount, sampleRate * encoding.BitsPerSample / 8, encoding.BlockAlign, null);

            _engine = new SpeechRecognitionEngine(new CultureInfo(languageCode));

            _engine.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(RecognizeCompleted);

            _engine.SpeechRecognitionRejected +=
                new EventHandler<SpeechRecognitionRejectedEventArgs>(SpeechRecognitionRejected);

            Choices options = new Choices();
            options.Add(new string[] {
                "sim",
                "não",
                "alô"
            });

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(options);

            Grammar g = new Grammar(gb);
            _engine.LoadGrammar(g);

            _engine.SetInputToAudioStream(
                new MemoryStream(ConvertAudioToBytes(audio_base64)), _info);
        }

        internal void Start()
        {
            _engine.RecognizeAsync();

            lock (_completed)
                Monitor.Wait(_completed);
        }
    }
}
