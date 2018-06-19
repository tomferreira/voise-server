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

        protected void ValidateArguments(AudioEncoding encoding, int sampleRate, string languageCode)
        {
            if (encoding == AudioEncoding.EncodingUnspecified)
                throw new BadEncodingException("Encoding is invalid.");

            if (sampleRate < 8000 || sampleRate > 48000)
                throw new BadEncodingException("Sample rate is invalid.");
        }

        protected ByteString ConvertAudioToByteString(string audio_base64)
        {
            if (audio_base64 == null || audio_base64.Trim() == string.Empty)
                throw new BadAudioException("Audio is empty.");

            try
            {
                return ByteString.CopyFrom(Convert.FromBase64String(audio_base64));
            }
            catch (System.Exception e)
            {
                throw new BadAudioException("Audio is invalid.", e);
            }
        }

        protected SpeechContext CreateSpeechContext(Dictionary<string, List<string>> contexts)
        {
            if (contexts == null || contexts.Count == 0)
                return null;

            var speechContext = new SpeechContext();

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
