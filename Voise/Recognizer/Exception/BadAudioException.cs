namespace Voise.Recognizer.Exception
{
    internal class BadAudioException : System.Exception
    {
        internal BadAudioException(string message)
            : base(message)
        {
        }

        internal BadAudioException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
