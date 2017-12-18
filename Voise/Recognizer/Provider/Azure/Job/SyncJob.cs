﻿using log4net;
using Microsoft.CognitiveServices.SpeechRecognition;
using System.Threading;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Azure.Job
{
    internal class SyncJob : Base, ISyncJob
    {
        private byte[] _audio;

        internal SyncJob(string primaryKey, string audio_base64, AudioEncoding encoding, int sampleRate, string languageCode)
            : base(LogManager.GetLogger(typeof(SyncJob)))
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _recognitionClient = SpeechRecognitionServiceFactory.CreateDataClient(
                SpeechRecognitionMode.ShortPhrase, // Audio up to 15 seconds
                languageCode,
                primaryKey);

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

        public void Start()
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
