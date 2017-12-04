﻿using Microsoft.CognitiveServices.SpeechRecognition;
using System.Threading;
using Voise.Synthesizer.Azure;
using static Voise.AudioStream;

namespace Voise.Recognizer.Azure.Job
{
    internal class StreamingJob : Base
    {
        private AudioStream _streamIn;

        internal StreamingJob(string primaryKey, AudioStream streamIn, AudioEncoding encoding, int sampleRate, string languageCode)
            : base()
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _recognitionClient = SpeechRecognitionServiceFactory.CreateDataClient(
                SpeechRecognitionMode.ShortPhrase, // Áudio de até 15 segundos
                languageCode,
                primaryKey);

            _recognitionClient.OnResponseReceived += ResponseReceivedHandler;
            _recognitionClient.OnConversationError += ConversationErrorHandler;

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

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
        }

        internal void Start()
        {
            _streamIn.Start();
        }

        internal void Stop()
        {
            _streamIn.Stop();

            _recognitionClient.EndAudio();

            lock (_monitorCompleted)
            {
                if (!_completed)
                    Monitor.Wait(_monitorCompleted);
            }
        }

        private void ConsumeStreamData(object sender, StreamInEventArgs e)
        {
            _recognitionClient.SendAudio(e.Buffer, e.BytesStreamed);
        }
    }
}