using System;
using System.Threading.Tasks;
using Voise.Tuning;

namespace Voise.Recognizer.Provider.Common.Job
{
    internal interface ISyncJob : IDisposable
    {
        SpeechRecognitionResult BestAlternative { get; }

        Task StartAsync(TuningIn tuning);
    }
}
