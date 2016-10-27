using log4net;
using System;
using Voise.Recognizer.Google;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamStopRequest : ProcessBase
    {
        internal ProcessStreamStopRequest(ClientConnection client, VoiseStreamRecognitionStopRequest request,
            GoogleRecognizer recognizer)
        {
            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Debug("StreamStopRequest");

            try
            {
                var recognition =
                    recognizer.StopStreamingRecognitionAsync(client.StreamIn).Result;

                client.CurrentPipeline.SpeechResult.Transcript = recognition.Transcript;
                client.CurrentPipeline.SpeechResult.Confidence = recognition.Confidence;
            }
            catch(Exception e)
            {
                client.CurrentPipeline.AsyncStreamError = e.InnerException;
                log.Error(e.InnerException?.Message);
            }

            // Avisa à pipeline que pode continuar.
            client.CurrentPipeline.ReleaseMutex();
        }
    }
}
