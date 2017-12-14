using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using Voise.Google.Cloud.Speech.V1Beta1;
using Voise.Recognizer.Provider.Common.Job;
using static Google.Cloud.Speech.V1Beta1.RecognitionConfig.Types;

namespace Voise.Recognizer.Provider.Google.Job
{
    internal class SyncJob : Base, ISyncJob
    {
        private SyncRecognizeRequest _config;

        internal SyncJob(SpeechRecognizer recognizer, string audio_base64, AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
            : base(recognizer)
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _config = new SyncRecognizeRequest
            {
                Config = new RecognitionConfig
                {
                    Encoding = encoding,
                    SampleRate = sampleRate,
                    MaxAlternatives = 5,
                    LanguageCode = languageCode,
                    SpeechContext = CreateSpeechContext(contexts)
                },
                Audio = new RecognitionAudio()
                {
                    Content = ConvertAudioToByteString(audio_base64)
                }
            };
        }

        public void Start()
        {
            SyncRecognizeResponse response = _recognizer.RecognizeAsync(_config.Config, _config.Audio).Result;

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    if (BestAlternative == SpeechRecognitionResult.NoResult || BestAlternative.Confidence < alternative.Confidence)
                        BestAlternative = new SpeechRecognitionResult(alternative.Transcript, alternative.Confidence);
                }
            }
        }
    }
}
