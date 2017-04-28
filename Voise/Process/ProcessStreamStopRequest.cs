﻿using log4net;
using System;
using Voise.Recognizer;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamStopRequest : ProcessBase
    {
        internal ProcessStreamStopRequest(ClientConnection client, VoiseStreamRecognitionStopRequest request,
            RecognizerManager recognizerManager)
        {
            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            try
            {
                if (client.CurrentPipeline.Recognizer == null)
                    throw new Exception("Engine not defined.");

                var recognition =
                    client.CurrentPipeline.Recognizer.StopStreamingRecognitionAsync(client.StreamIn).Result;

                client.CurrentPipeline.SpeechResult.Transcript = recognition.Transcript;
                client.CurrentPipeline.SpeechResult.Confidence = recognition.Confidence;
            }
            catch(Exception e)
            {
                client.CurrentPipeline.AsyncStreamError = e.InnerException ?? e;
                log.Error(e.InnerException?.Message);
            }

            // Avisa à pipeline que pode continuar.
            client.CurrentPipeline.ReleaseMutex();
        }
    }
}
