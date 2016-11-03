using log4net;
using System;
using Voise.Classification;
using Voise.Recognizer.Google;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamStartRequest : ProcessBase
    {
        internal ProcessStreamStartRequest(ClientConnection client, VoiseStreamRecognitionStartRequest request,
            GoogleRecognizer recognizer, ClassifierManager classifierManager)
        {
            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Debug("StreamStartRequest");

            // This client already is streaming audio.
            if (client.StreamIn != null)
            {
                SendError(client, new Exception("Client already is streaming audio."));
                return;
            }

            var pipeline = client.CurrentPipeline = new Pipeline();

            var task1 = pipeline.StartNew(async () =>
            {
                try
                { 
                    client.StreamIn = new AudioStream(100, request.Config.sample_rate, 2);

                    await recognizer.StartStreamingRecognitionAsync(
                        client.StreamIn,
                        GoogleRecognizer.ConvertAudioEncoding(request.Config.encoding),
                        request.Config.sample_rate,
                        request.Config.language_code,
                        request.Config.context);

                    SendAccept(client);

                    pipeline.SpeechResult = new SpeechResult(SpeechResult.Modes.ASR);
                }
                catch (Exception e)
                {
                    SendError(client, e);
                    pipeline.CancelExecution();
                }
            });

            var task2 = pipeline.StartNew(async () =>
            {
                // Espera pelo término do streaming para continuar a pipeline.
                // Veja o tratamento do comando 'StreamDataRequest'.
                await pipeline.WaitAsync();

                // Caso ocorra alguma exceção asíncrona durante o streming do áudio
                if (pipeline.AsyncStreamError != null)
                {
                    SendError(client, pipeline.AsyncStreamError);
                    pipeline.CancelExecution();
                }
            });

            var task3 = pipeline.StartNew(async () =>
            {
                if (request.Config.model_name == null || pipeline.SpeechResult.Transcript == null)
                    return;

                if (pipeline.SpeechResult.Transcript == NoResultSpeechRecognitionAlternative.Default.Transcript)
                {
                    pipeline.SpeechResult.Intent = NoResultSpeechRecognitionAlternative.Default.Transcript;
                    pipeline.SpeechResult.Probability = 1;

                    return;
                }

                try
                { 
                    var classification = await classifierManager.ClassifyAsync(
                        request.Config.model_name,
                        pipeline.SpeechResult.Transcript);

                    pipeline.SpeechResult.Intent = classification.ClassName;
                    pipeline.SpeechResult.Probability = classification.Probability;
                }
                catch (Classification.Exception.BadModelException e)
                {
                    SendError(client, e);
                    pipeline.CancelExecution();
                }
                catch (Exception e)
                {
                    SendError(client, e);
                    pipeline.CancelExecution();
                }
            });

            var task4 = pipeline.StartNew(async () =>
            {
                // Cleanup streamIn
                client.StreamIn = null;

                SendResult(client, pipeline.SpeechResult);
                pipeline = client.CurrentPipeline = null;
            });

            pipeline.WaitAll(task1, task2, task3, task4);
        }
    }
}
