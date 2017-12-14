using System;

namespace Voise.Recognizer.Common.Job
{
    internal interface ISyncJob : IDisposable
    {
        SpeechRecognitionResult BestAlternative { get; }

        void Start();
    }
}
