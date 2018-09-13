using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Classification;
using Voise.General;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.TCP.Response;

namespace Voise.Process
{
    internal abstract class ProcessBase
    {
        protected readonly ClientConnection _client;

        internal ProcessBase(ClientConnection client)
        {
            _client = client;
        }

        internal abstract Task ExecuteAsync();

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

        protected void SendResult(VoiseResult result)
        {
            if (result != null)
            {
                var response = new VoiseResponse(ResponseCode.OK);

                switch (result.Mode)
                {
                    case VoiseResult.Modes.ASR:
                        response.utterance = result.Transcript;
                        response.confidence = result.Confidence;
                        response.intent = result.Intent;
                        response.probability = result.Probability;
                        break;

                    case VoiseResult.Modes.TTS:
                        response.audio = new VoiseResponse.VoiseAudio();
                        response.audio.content = result.AudioContent;
                        response.audio.length = result.AudioContent.Length;
                        break;

                    default:
                        throw new Exception("Invalid mode result.");
                }

                _client?.SendResponse(response);
            }
            else
            {
                var response = new VoiseResponse(ResponseCode.NORESULT);
                _client?.SendResponse(response);
            }
        }

        protected void SendAccept()
        {
            var response = new VoiseResponse(ResponseCode.ACCEPTED);
            _client?.SendResponse(response);
        }

        protected void SendError(Exception e)
        {
            var response = new VoiseResponse(ResponseCode.ERROR, e.Message);
            _client?.SendResponse(response);
        }
    }
}
