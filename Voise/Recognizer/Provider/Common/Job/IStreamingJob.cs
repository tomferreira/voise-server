using System;

namespace Voise.Recognizer.Provider.Common.Job
{
    internal interface IStreamingJob : IDisposable
    {
        SpeechRecognitionResult BestAlternative { get; }

        void Start();
        void Stop();
    }
}
