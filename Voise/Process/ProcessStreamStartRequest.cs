using System;
using System.Collections.Generic;
using Voise.Classification;
using Voise.Recognizer;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamStartRequest : ProcessBase
    {
        internal static async void Execute(ClientConnection client, VoiseStreamRecognitionStartRequest request,
            RecognizerManager recognizerManager, ClassifierManager classifierManager)
        {
            // This client already is streaming audio.
            if (client.StreamIn != null)
            {
                SendError(client, new Exception("Client already is streaming audio."));
                return;
            }

            var pipeline = client.CurrentPipeline = new Pipeline();

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

                pipeline.SpeechResult = new SpeechResult(SpeechResult.Modes.ASR);
            }
            catch (Exception e)
            {
                // Cleanup streamIn
                client.StreamIn = null;

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

                SendError(client, pipeline.AsyncStreamError);
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

                SendResult(client, pipeline.SpeechResult);
            }
            catch (Exception e)
            {

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
