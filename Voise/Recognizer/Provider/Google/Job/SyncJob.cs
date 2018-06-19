using Google.Cloud.Speech.V1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Google.Internal;
using static Google.Cloud.Speech.V1.RecognitionConfig.Types;

namespace Voise.Recognizer.Provider.Google.Job
{
    internal class SyncJob : Base, ISyncJob
    {
        private RecognizeRequest _request;

        internal SyncJob(SpeechRecognizer recognizer, string audio_base64, AudioEncoding encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
            : base(recognizer)
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _request = new RecognizeRequest
            {
                Config = new RecognitionConfig
                {
                    Encoding = encoding,
                    SampleRateHertz = sampleRate,
                    MaxAlternatives = 5,
                    LanguageCode = languageCode,
                    SpeechContexts = { CreateSpeechContext(contexts) }
                },
                Audio = new RecognitionAudio()
                {
                    Content = ConvertAudioToByteString(audio_base64)
                }
            };
        }

        public async Task StartAsync()
        {
            RecognizeResponse response =
                await _recognizer.RecognizeAsync(_request.Config, _request.Audio).ConfigureAwait(false);

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
