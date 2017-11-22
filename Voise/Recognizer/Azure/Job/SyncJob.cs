using log4net;
using Microsoft.CognitiveServices.SpeechRecognition;
using System.Threading;
using Voise.Synthesizer.Azure;

namespace Voise.Recognizer.Azure.Job
{
    internal class SyncJob : Base
    {
        private byte[] _audio;

        internal SyncJob(string audio_base64, AudioEncoding encoding, int sampleRate, string languageCode)
        {
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            ValidateArguments(encoding, sampleRate, languageCode);

            _recognitionClient = SpeechRecognitionServiceFactory.CreateDataClient(
                SpeechRecognitionMode.ShortPhrase, // Áudio de até 15 segundos
                languageCode,
                "b2abf005d1e24bde8821f26d5cc14710");

            _recognitionClient.OnResponseReceived += ResponseReceivedHandler;
            _recognitionClient.OnConversationError += ConversationErrorHandler;

            _audio = Util.ConvertAudioToBytes(audio_base64);

            SpeechAudioFormat format = new SpeechAudioFormat()
            {
                EncodingFormat = encoding.Format,
                SamplesPerSecond = sampleRate,
                BitsPerSample = encoding.BitsPerSample,
                ChannelCount = encoding.ChannelCount,
                AverageBytesPerSecond = sampleRate * encoding.BitsPerSample / 8,
                BlockAlign = encoding.BlockAlign
            };

            _recognitionClient.SendAudioFormat(format);
        }

        internal void Start()
        {
            _recognitionClient.SendAudio(_audio, _audio.Length);
            _recognitionClient.EndAudio();

            lock (_monitorCompleted)
            {
                if (!_completed)
                    Monitor.Wait(_monitorCompleted);
            }
        }
    }
}
