using System;
using System.Threading.Tasks;

namespace Voise.Recognizer.Provider.Common.Job
{
    internal interface ISyncJob : IDisposable
    {
        SpeechRecognitionResult BestAlternative { get; }

        Task StartAsync();
    }
}
