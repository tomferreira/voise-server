using System;
using System.Threading.Tasks;

namespace Voise.Synthesizer.Provider.Common.Job
{
    internal interface IJob : IDisposable
    {
        Task SynthAsync(string text);
    }
}
