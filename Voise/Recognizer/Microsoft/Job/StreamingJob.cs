using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Voise.Recognizer.Microsoft.Internal;
using Voise.Synthesizer.Microsoft;
using static Voise.AudioStream;

namespace Voise.Recognizer.Microsoft.Job
{
    internal class StreamingJob : Base
    {
        private AudioStream _streamIn;

        private SpeechStreamer _ss;

        internal StreamingJob(AudioStream streamIn, AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
            : base()
        {
            ValidateArguments(encoding, sampleRate, languageCode, contexts);

            SpeechAudioFormatInfo info = new SpeechAudioFormatInfo(encoding.Format, sampleRate, encoding.BitsPerSample,
                encoding.ChannelCount, sampleRate * encoding.BitsPerSample / 8, encoding.BlockAlign, null);

            CultureInfo cultureInfo = new CultureInfo(languageCode);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            _engine = new SpeechRecognitionEngine(cultureInfo);

            _engine.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(RecognizeCompleted);

            // Not reject any utterance
            _engine.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 0);

            foreach (var context in contexts)
            {
                GrammarBuilder gb = new GrammarBuilder();
                gb.Culture = cultureInfo;
                gb.Append(new Choices(context.Value.ToArray()));                

                Grammar gram = new Grammar(gb);
                gram.Name = context.Key;

                _engine.LoadGrammar(gram);
            }

            // Prevents the event RecognizeCompleted to be triggered improperly
            _engine.EndSilenceTimeout = TimeSpan.FromSeconds(10);

            _ss = new SpeechStreamer(streamIn.BufferCapacity * 50);

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
            _streamIn.StreamingStopped += StreamingStopped;

            _engine.SetInputToAudioStream(_ss, info);
        }

        internal void Start()
        {
            _streamIn.Start();

            _engine.RecognizeAsync(RecognizeMode.Single);
        }

        private void StreamingStopped(object sender, EventArgs e)
        {
            _ss.Complete();
        }

        private void ConsumeStreamData(object sender, StreamInEventArgs e)
        {
            _ss.Write(e.Buffer, 0, e.BytesStreamed);
        }

        internal void Stop()
        {
            _streamIn.Stop();

            _engine.RecognizeAsyncStop();

            lock (_monitorCompleted)
            {
                if (!_completed)
                    Monitor.Wait(_monitorCompleted);
            }

            _ss.Close();
        }
    }
}
