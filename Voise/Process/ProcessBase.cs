using System;
using Voise.TCP;
using Voise.TCP.Response;

namespace Voise.Process
{
    internal abstract class ProcessBase
    {
        protected void SendResult(ClientConnection client, SpeechResult result)
        {
            if (result != null)
            {
                var response = new VoiseResponse(200, "OK");

                response.utterance = result.Transcript;
                response.confidence = result.Confidence;
                response.intent = result.Intent;
                response.probability = result.Probability;

                client?.SendResponse(response);
            }
            else
            {
                var response = new VoiseResponse(202, "No result");
                client?.SendResponse(response);
            }            
        }

        protected void SendAccept(ClientConnection client)
        {
            var response = new VoiseResponse(201, "Accept");
            client?.SendResponse(response);
        }

        protected void SendError(ClientConnection client, Exception e)
        {
            var response = new VoiseResponse(300, e.Message);
            client?.SendResponse(response);
        }
    }
}
