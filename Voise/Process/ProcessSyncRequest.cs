using log4net;
using System;
using System.Collections.Generic;
using Voise.Classification;
using Voise.Recognizer;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessSyncRequest : ProcessBase
    {
        internal static async void Execute(ClientConnection client, VoiseSyncRecognitionRequest request,
            RecognizerManager recognizerManager, ClassifierManager classifierManager)
        {
            ILog log = LogManager.GetLogger(typeof(ProcessStreamStartRequest));

            var pipeline = client.CurrentPipeline = new Pipeline();

            log.Info($"Starting request with engine '{request.Config.engine_id}' at pipeline {pipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

            try
            {
                Recognizer.Base recognizer = recognizerManager.GetRecognizer(request.Config.engine_id);

                Dictionary<string, List<string>> contexts = GetContexts(request.Config, classifierManager);

                var recognition = await recognizer.SyncRecognition(
                    request.audio,
                    request.Config.encoding,
                    request.Config.sample_rate,
                    request.Config.language_code,
                    contexts);

                //
                pipeline.SpeechResult = new SpeechResult(SpeechResult.Modes.ASR);
                pipeline.SpeechResult.Transcript = recognition.Transcript;
                pipeline.SpeechResult.Confidence = recognition.Confidence;
            }
            catch (Exception e)
            {
                log.Error($"{e.Message}. [Client: {client.RemoteEndPoint().ToString()}]");

                SendError(client, e);
                return;
            }

            try
            {
                if (request.Config.model_name != null && pipeline.SpeechResult.Transcript != null)
                {
                    if (pipeline.SpeechResult.Transcript == NoResultSpeechRecognitionAlternative.Default.Transcript)
                    {
                        pipeline.SpeechResult.Intent = NoResultSpeechRecognitionAlternative.Default.Transcript;
                        pipeline.SpeechResult.Probability = 1;
                    }
                    else
                    {
                        var classification = await classifierManager.ClassifyAsync(
                            request.Config.model_name,
                            pipeline.SpeechResult.Transcript);

                        pipeline.SpeechResult.Intent = classification.ClassName;
                        pipeline.SpeechResult.Probability = classification.Probability;
                    }
                }

                log.Info($"Request successful finished at pipeline {pipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

                SendResult(client, pipeline.SpeechResult);
            }
            catch (Exception e)
            {
                log.Error($"{e.Message}. [Client: {client.RemoteEndPoint().ToString()}]");

                SendError(client, e);
            }
            finally
            {
                pipeline = client.CurrentPipeline = null;
            }
        }
    }
}
