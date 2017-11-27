
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voise.Recognizer
{
    internal abstract class Base
    {
        internal abstract Task<SpeechRecognitionResult> SyncRecognition(string audio_base64, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);

        internal abstract Task StartStreamingRecognitionAsync(AudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);

        internal abstract Task<SpeechRecognitionResult> StopStreamingRecognitionAsync(AudioStream streamIn);
    }
}
