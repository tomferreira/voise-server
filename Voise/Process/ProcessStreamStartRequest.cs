using log4net;
using System;
using System.Collections.Generic;
using Voise.Classification;
using Voise.Recognizer;
using Voise.Recognizer.Exception;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamStartRequest : ProcessBase
    {
        internal static async void Execute(ClientConnection client, VoiseStreamRecognitionStartRequest request,
            RecognizerManager recognizerManager, ClassifierManager classifierManager)
        {
            ILog log = LogManager.GetLogger(typeof(ProcessStreamStartRequest));

            // This client already is streaming audio.
            if (client.StreamIn != null)
            {
                log.Error($"Client already is streaming audio. [Client: {client.RemoteEndPoint().ToString()}]");

                SendError(client, new Exception("Client already is streaming audio."));
                return;
            }

            var pipeline = client.CurrentPipeline = new Pipeline();

            log.Info($"Starting stream request with engine '{request.Config.engine_id}' at pipeline {pipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

            try
            {
                Recognizer.Base recognizer = recognizerManager.GetRecognizer(request.Config.engine_id);

                // Set the recognizer for be used when to stop the stream
                client.CurrentPipeline.Recognizer = recognizer;

                Dictionary<string, List<string>> contexts = GetContexts(request.Config, classifierManager);

                // FIXME
                ////int bytesPerSample = GoogleRecognizer.GetBytesPerSample(request.Config.encoding);
                int bytesPerSample = 2;

                client.StreamIn = new AudioStream(100, request.Config.sample_rate, bytesPerSample);

                await recognizer.StartStreamingRecognitionAsync(
                    client.StreamIn,
                    request.Config.encoding,
                    request.Config.sample_rate,
                    request.Config.language_code,
                    contexts);

                SendAccept(client);

                pipeline.Result = new VoiseResult(VoiseResult.Modes.ASR);
            }
            catch (Exception e)
            {
                // Cleanup streamIn
                client.StreamIn = null;

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

            // Espera pelo término do streaming para continuar a pipeline.
            // Veja o tratamento do comando 'StreamDataRequest'.
            await pipeline.WaitAsync();

            // Caso ocorra alguma exceção asíncrona durante o streming do áudio
            if (pipeline.AsyncStreamError != null)
            {
                // Cleanup streamIn
                client.StreamIn = null;

                log.Error($"{pipeline.AsyncStreamError.Message}. [Client: {client.RemoteEndPoint().ToString()}]");

                SendError(client, pipeline.AsyncStreamError);
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

                log.Info($"Stream request successful finished at pipeline {pipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

                SendResult(client, pipeline.Result);
            }
            catch (Exception e)
            {
                log.Error($"{e.Message}\nStacktrace: {e.StackTrace}. [Client: {client.RemoteEndPoint().ToString()}]");

                SendError(client, e);
            }
            finally
            {
                // Cleanup streamIn
                client.StreamIn = null;

                pipeline = client.CurrentPipeline = null;
            }
        }
    }
}
