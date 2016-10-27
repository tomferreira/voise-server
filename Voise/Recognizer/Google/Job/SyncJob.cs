using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Google.Cloud.Speech.V1Beta1;
using static Google.Cloud.Speech.V1Beta1.RecognitionConfig.Types;

namespace Voise.Recognizer.Google.Job
{
    internal class SyncJob : Base
    {
        private SyncRecognizeRequest _config;

        internal SyncJob(string audio_base64, AudioEncoding encoding, int sampleRate, string languageCode, List<string> context)
            : base()
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
                    SpeechContext = CreateSpeechContext(context)
                },
                Audio = new RecognitionAudio()
                {
                    Content = ConvertAudioToByteString(audio_base64)
                }
            };
        }

        internal async Task StartAsync(SpeechRecognizer recognizer)
        {
            SyncRecognizeResponse response = await recognizer.RecognizeAsync(_config.Config, _config.Audio);

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    if (BestAlternative == null || BestAlternative.Confidence < alternative.Confidence)
                        BestAlternative = alternative;
                }
            }
        }
    }
}
