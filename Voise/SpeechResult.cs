namespace Voise
{
    internal class SpeechResult
    {
        public string Transcript { get; set; }
        public float Confidence { get; set; }
        public string Intent { get; set; }
        public float Probability { get; set; }
    }
}
