using System;
using System.Collections.Generic;
using Voise.Classification;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.TCP.Response;

namespace Voise.Process
{
    internal abstract class ProcessBase
    {
        protected Dictionary<string, List<string>> GetContexts(VoiseConfig config, ClassifierManager classifierManager)
        {
            Dictionary<string, List<string>> contexts = null;

            if (config.context != null)
            {
                contexts = new Dictionary<string, List<string>>();
                contexts.Add("default", config.context);
            }
            else
            {
                contexts = classifierManager.GetTrainingList(config.model_name);
            }

            return contexts;
        }

        protected void SendResult(ClientConnection client, SpeechResult result)
        {
            if (result != null)
            {
                var response = new VoiseResponse(ResponseCode.OK);

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
                var response = new VoiseResponse(ResponseCode.NORESULT);
                client?.SendResponse(response);
            }            
        }

        protected void SendAccept(ClientConnection client)
        {
            var response = new VoiseResponse(ResponseCode.ACCEPTED);
            client?.SendResponse(response);
        }

        protected void SendError(ClientConnection client, Exception e)
        {
            var response = new VoiseResponse(ResponseCode.ERROR, e.Message);
            client?.SendResponse(response);
        }
    }
}
