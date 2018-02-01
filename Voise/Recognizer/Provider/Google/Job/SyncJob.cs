using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Google.Internal;
using Voise.Tuning;
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

        public async Task StartAsync(TuningIn tuning)
        {
            _tuning = tuning;

            if (_tuning != null)
            {
                byte[] audio = _config.Audio.Content.ToByteArray();
                _tuning?.WriteRecording(audio, 0, audio.Length);
            }

           SyncRecognizeResponse response = await _recognizer.RecognizeAsync(_config.Config, _config.Audio);

            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    if (BestAlternative == SpeechRecognitionResult.NoResult || BestAlternative.Confidence < alternative.Confidence)
                        BestAlternative = new SpeechRecognitionResult(alternative.Transcript, alternative.Confidence);
                }
            }

            _tuning?.SaveSpeechRecognitionResult(BestAlternative);
        }
    }
}
