﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Classification.Interface;
using Voise.General;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.TCP.Response;

namespace Voise.Process
{
    internal abstract class ProcessBase
    {
        protected readonly IClientConnection _client;

        protected ProcessBase(IClientConnection client)
        {
            _client = client;
        }

        internal abstract Task ExecuteAsync();

        protected static Dictionary<string, List<string>> GetContexts(VoiseConfig config, IClassifierManager classifierManager)
        {
            Dictionary<string, List<string>> contexts = null;

            if (config.Context != null)
            {
                contexts = new Dictionary<string, List<string>>
                {
                    { "default", config.Context }
                };
            }
            else
            {
                contexts = classifierManager.GetTrainingList(
                    config.ModelName, config.LanguageCode );
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
                        response.audio = new VoiseResponse.VoiseAudio
                        {
                            content = result.AudioContent,
                            length = result.AudioContent.Length
                        };
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
