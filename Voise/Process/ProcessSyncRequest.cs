using log4net;
using System;
using System.Collections.Generic;
using Voise.Classification;
using Voise.Recognizer;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Exception;
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
                CommonRecognizer recognizer = recognizerManager.GetRecognizer(request.Config.engine_id);

                Dictionary<string, List<string>> contexts = GetContexts(request.Config, classifierManager);

                var recognition = await recognizer.SyncRecognition(
                    request.audio,
                    request.Config.encoding,
                    request.Config.sample_rate,
                    request.Config.language_code,
                    contexts);

                //
                pipeline.Result = new VoiseResult(VoiseResult.Modes.ASR);
                pipeline.Result.Transcript = recognition.Transcript;
                pipeline.Result.Confidence = recognition.Confidence;
            }
            catch (Exception e)
            {
                if (e is BadEncodingException || e is BadAudioException)
                {
                    log.Info($"{e.Message} [Client: {client.RemoteEndPoint().ToString()}]");
                }
                else
                {
                    log.Error($"{e.Message}\nStacktrace: {e.StackTrace}. [Client: {client.RemoteEndPoint().ToString()}]");
                }

                SendError(client, e);
                return;
            }

            try
            {
                if (request.Config.model_name != null && pipeline.Result.Transcript != null)
                {
                    if (pipeline.Result.Transcript == SpeechRecognitionResult.NoResult.Transcript)
                    {
                        pipeline.Result.Intent = SpeechRecognitionResult.NoResult.Transcript;
                        pipeline.Result.Probability = 1;
                    }
                    else
                    {
                        var classification = await classifierManager.ClassifyAsync(
                            request.Config.model_name,
                            pipeline.Result.Transcript);

                        pipeline.Result.Intent = classification.ClassName;
                        pipeline.Result.Probability = classification.Probability;
                    }
                }

                log.Info($"Request successful finished at pipeline {pipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

                SendResult(client, pipeline.Result);
            }
            catch (Exception e)
            {
                log.Error($"{e.Message}\nStacktrace: {e.StackTrace}. [Client: {client.RemoteEndPoint().ToString()}]");

                SendError(client, e);
            }
            finally
            {
                pipeline = client.CurrentPipeline = null;
            }
        }
    }
}
