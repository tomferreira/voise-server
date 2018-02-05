using log4net;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Microsoft.Internal;
using Voise.Synthesizer.Microsoft;
using static Voise.AudioStream;

namespace Voise.Recognizer.Provider.Microsoft.Job
{
    internal class StreamingJob : Base, IStreamingJob
    {
        private AudioStream _streamIn;
        private SpeechStreamer _ss;

        private SpeechAudioFormatInfo _info;

        internal StreamingJob(AudioStream streamIn, AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
            : base(LogManager.GetLogger(typeof(StreamingJob)))
        {
            ValidateArguments(encoding, sampleRate, languageCode, contexts);

            _info = new SpeechAudioFormatInfo(encoding.Format, sampleRate, encoding.BitsPerSample,
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

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
            _streamIn.StreamingStopped += StreamingStopped;
        }

        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                _ss = new SpeechStreamer(_streamIn.BufferCapacity * 50);
                _engine.SetInputToAudioStream(_ss, _info);

                _streamIn.Start();

                _engine.RecognizeAsync(RecognizeMode.Single);
            });
        }

        private void StreamingStopped(object sender, EventArgs e)
        {
            _ss.Complete();
        }

        private void ConsumeStreamData(object sender, StreamInEventArgs e)
        {
            try
            {
                _ss.Write(e.Buffer, 0, e.BytesStreamed);
            }
            catch(System.Exception ex)
            {
                _log.Error($"There was a problem writing the chunck of audio on the speach streamer: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            await Task.Run(() =>
            {
                _streamIn.Stop();

                _engine.RecognizeAsyncStop();

                lock (_monitorCompleted)
                {
                    if (!_completed)
                        Monitor.Wait(_monitorCompleted);
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _ss.Close();

            base.Dispose(disposing);
        }
    }
}
