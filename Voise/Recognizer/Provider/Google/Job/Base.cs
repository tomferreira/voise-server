using Google.Cloud.Speech.V1;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using Voise.Recognizer.Exception;
using Voise.Recognizer.Provider.Google.Internal;
using static Google.Cloud.Speech.V1.RecognitionConfig.Types;

namespace Voise.Recognizer.Provider.Google.Job
{
    internal abstract class Base : IDisposable
    {
        protected SpeechRecognizer _recognizer;

        protected bool _disposed;

        public SpeechRecognitionResult BestAlternative { get; protected set; }

        protected Base(SpeechRecognizer recognizer)
        {
            _recognizer = recognizer;

            _disposed = false;

            // Set as default alternative
            BestAlternative = SpeechRecognitionResult.NoResult;
        }

        protected static void ValidateArguments(AudioEncoding encoding, int sampleRate, string languageCode)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");
        }

        protected static ByteString ConvertAudioToByteString(byte[] audio)
        {
            try
            {
                return ByteString.CopyFrom(audio);
            }
            catch (System.Exception e)
            {
                throw new BadAudioException("Audio is invalid.", e);
            }
        }

        protected static SpeechContext CreateSpeechContext(Dictionary<string, List<string>> contexts)
        {
            var speechContext = new SpeechContext();

            if (contexts == null || contexts.Count == 0)
                return speechContext;

            foreach (var context in contexts)
            {
                foreach (var phrase in context.Value)
                    speechContext.Phrases.Add(phrase);
            }

            return speechContext;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // Do nothing.

            _disposed = true;
        }
    }
}
