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
            var pipeline = client.CurrentPipeline = new Pipeline();

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
                SendError(client, e);
                return;
            }

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
                return;
            }
            catch (Exception e)
            {
                SendError(client, e);
                return;
            }

            SendResult(client, pipeline.SpeechResult);
            pipeline = client.CurrentPipeline = null;
        }
    }
}
