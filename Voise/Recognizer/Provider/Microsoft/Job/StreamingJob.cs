using log4net;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Voise.General.Interface;
using Voise.Provider.Microsoft;
using Voise.Recognizer.Exception;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Microsoft.Internal;

namespace Voise.Recognizer.Provider.Microsoft.Job
{
    internal class StreamingJob : Base, IStreamingJob
    {
        private readonly IAudioStream _streamIn;
        private readonly SpeechAudioFormatInfo _info;

        private SpeechStreamer _ss;

        internal StreamingJob(IAudioStream streamIn, AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
            : base(LogManager.GetLogger(typeof(StreamingJob)))
        {
            ValidateArguments(encoding, sampleRate, languageCode, contexts);

            _info = new SpeechAudioFormatInfo(encoding.Format, sampleRate, encoding.BitsPerSample,
                encoding.ChannelCount, sampleRate * encoding.BitsPerSample / 8, encoding.BlockAlign, null);

            RecognizerInfo recognizerInfo = GetInstalledRecognizerInfo(languageCode, false);

            if (recognizerInfo == null)
                throw new LanguageCodeNotSupportedException($"Recognizer info not found for language '{languageCode}'");

            Thread.CurrentThread.CurrentCulture = recognizerInfo.Culture;
            _engine = new SpeechRecognitionEngine(recognizerInfo);

            _engine.RecognizeCompleted +=
                new EventHandler<RecognizeCompletedEventArgs>(RecognizeCompleted);

            // Not reject any utterance
            _engine.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 0);

            foreach (var context in contexts)
            {
                GrammarBuilder gb = new GrammarBuilder()
                {
                    Culture = _engine.RecognizerInfo.Culture
                };

                gb.Append(new Choices(context.Value.ToArray()));

                Grammar gram = new Grammar(gb)
                {
                    Name = context.Key
                };

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
                _ss = new SpeechStreamer(_streamIn.BufferCapacity * 150);
                _engine.SetInputToAudioStream(_ss, _info);

                _streamIn.Start();

                _engine.RecognizeAsync(RecognizeMode.Single);
            }).ConfigureAwait(false);
        }

        private void StreamingStopped(object sender, EventArgs e)
        {
            _ss.Complete();
        }

        private void ConsumeStreamData(object sender, IStreamInEventArgs e)
        {
            try
            {
                _ss.Write(e.Buffer, 0, e.BytesStreamed);
            }
            catch (SpeechStreamer.BufferOverwrittenException)
            {
                _log.Warn("SpeechStreamer's buffer has overwritten. It's recommended increase buffer size.");
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
            }).ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _ss.Dispose();

            base.Dispose(disposing);
        }
    }
}
