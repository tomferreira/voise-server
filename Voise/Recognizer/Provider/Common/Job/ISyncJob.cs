using System;

namespace Voise.Recognizer.Provider.Common.Job
{
    internal interface ISyncJob : IDisposable
    {
        SpeechRecognitionResult BestAlternative { get; }

        void Start();
    }
}
