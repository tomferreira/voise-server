using log4net;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voise.Provider.Microsoft;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Microsoft.Job
{
    internal class SyncJob : Base, ISyncJob
    {
        internal SyncJob(string audio_base64, AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
            : base(LogManager.GetLogger(typeof(SyncJob)))
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
                GrammarBuilder gb = new GrammarBuilder
                {
                    Culture = cultureInfo
                };

                gb.Append(new Choices(context.Value.ToArray()));

                Grammar gram = new Grammar(gb)
                {
                    Name = context.Key
                };

                _engine.LoadGrammar(gram);
            }

            _engine.SetInputToAudioStream(
                new MemoryStream(Util.ConvertAudioToBytes(audio_base64)), info);
        }

        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                _engine.RecognizeAsync();

                lock (_monitorCompleted)
                {
                    if (!_completed)
                        Monitor.Wait(_monitorCompleted);
                }
            }).ConfigureAwait(false);
        }
    }
}
