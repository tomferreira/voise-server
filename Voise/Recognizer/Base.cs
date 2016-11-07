
using Google.Cloud.Speech.V1Beta1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voise.Recognizer
{
    internal abstract class Base
    {
        internal abstract Task<SpeechRecognitionAlternative> SyncRecognition(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);

        internal abstract Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);

        internal abstract Task<SpeechRecognitionAlternative> StopStreamingRecognitionAsync(AudioStream streamIn);
    }
}
