using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.General.Interface;

namespace Voise.Recognizer.Provider.Common
{
    public interface ICommonRecognizer
    {
        Task<SpeechRecognitionResult> SyncRecognition(byte[] audio, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);

        Task StartStreamingRecognitionAsync(IAudioStream streamIn, string encoding,
            int sampleRate, string languageCode, Dictionary<string, List<string>> contexts);

        Task<SpeechRecognitionResult> StopStreamingRecognitionAsync(IAudioStream streamIn);
    }
}
