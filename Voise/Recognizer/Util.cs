using System;
using Voise.Recognizer.Exception;

namespace Voise.Recognizer
{
    internal class Util
    {
        internal static byte[] ConvertAudioToBytes(string audio_base64)
        {
            if (audio_base64 == null || audio_base64.Trim() == string.Empty)
                throw new BadAudioException("Audio is empty.");

            try
            {
                return Convert.FromBase64String(audio_base64);
            }
            catch (System.Exception e)
            {
                throw new BadAudioException("Audio is invalid.", e);
            }
        }
    }
}
