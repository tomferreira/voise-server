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

                switch(result.Mode)
                {
                    case SpeechResult.Modes.ASR:
                        response.utterance = result.Transcript;
                        response.confidence = result.Confidence;
                        response.intent = result.Intent;
                        response.probability = result.Probability;
                        break;

                    case SpeechResult.Modes.TTS:
                        response.audio = new VoiseResponse.VoiseAudio();
                        response.audio.content = result.AudioContent;
                        response.audio.length = result.AudioContent.Length;
                        break;

                    default:
                        throw new Exception("Invalid mode result.");
                }

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
