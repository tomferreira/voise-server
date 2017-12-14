using System;

namespace Voise.Recognizer.Common.Job
{
    internal interface IStreamingJob : IDisposable
    {
        SpeechRecognitionResult BestAlternative { get; }

        void Start();
        void Stop();
    }
}
