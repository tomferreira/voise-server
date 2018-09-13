using System;
using System.Threading.Tasks;

namespace Voise.Recognizer.Provider.Common.Job
{
    internal interface IStreamingJob : IDisposable
    { 
        SpeechRecognitionResult BestAlternative { get; }

        Task StartAsync();
        Task StopAsync();
    }
}
