using Google.Cloud.Speech.V1Beta1;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using Voise.Recognizer.Exception;
using static Google.Cloud.Speech.V1Beta1.RecognitionConfig.Types;

namespace Voise.Recognizer.Google.Job
{
    internal abstract class Base
    {
        public SpeechRecognitionAlternative BestAlternative { get; protected set; }

        protected Base()
        {
            // Set as default alterative
            BestAlternative = NoResultSpeechRecognitionAlternative.Default;
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
    }
}
