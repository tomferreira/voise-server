using System;
using System.Threading.Tasks;

namespace Voise.Synthesizer.Provider.Common.Job
{
    public interface IJob : IDisposable
    {
        Task SynthAsync(string text);
    }
}
