
namespace Voise.Synthesizer.Microsoft
{
    internal class SynthetizerVoice
    {
        internal static readonly SynthetizerVoice Maria = new SynthetizerVoice("Microsoft Maria Desktop");
        internal static readonly SynthetizerVoice Zira = new SynthetizerVoice("Microsoft Zira Desktop");

        internal string Name { get; private set; }

        private SynthetizerVoice(string name)
        {
            Name = name;
        }
    }
}
