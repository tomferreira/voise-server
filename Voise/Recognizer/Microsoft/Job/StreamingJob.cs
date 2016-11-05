using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Voise.Synthesizer.Microsoft;
using static Voise.AudioStream;

namespace Voise.Recognizer.Microsoft.Job
{
    internal class StreamingJob : Base
    {
        private AudioStream _streamIn;

        private SpeechStreamer _ss;

        internal StreamingJob(AudioStream streamIn, AudioEncoding encoding, int sampleRate, string languageCode, List<string> context)
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

            // Prevents the event RecognizeCompleted to be triggered improperly
            _engine.EndSilenceTimeout = TimeSpan.FromSeconds(10);

            _ss = new SpeechStreamer(3200);

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
            _streamIn.StreamingStopped += StreamingStopped;

            _engine.SetInputToAudioStream(_ss, _info);
        }

        internal void Start()
        {
            _streamIn.Start();

            _engine.RecognizeAsync(RecognizeMode.Single);
        }

        private async void StreamingStopped(object sender, EventArgs e)
        {
            //await _requestQueue.CompleteAsync();
            _ss.Close();
        }

        private void ConsumeStreamData(object sender, StreamInEventArgs e)
        {
            _ss.Write(e.Buffer, 0, e.BytesStreamed);
        }

        internal void Stop()
        {
            _streamIn.Stop();

            _engine.RecognizeAsyncStop();

            lock (_completed)
                Monitor.Wait(_completed);
        }
    }
}
