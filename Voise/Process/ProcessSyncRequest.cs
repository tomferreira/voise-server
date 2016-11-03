using log4net;
using System;
using Voise.Classification;
using Voise.Recognizer.Google;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessSyncRequest : ProcessBase
    {
        internal ProcessSyncRequest(ClientConnection client, VoiseSyncRecognitionRequest request,
            GoogleRecognizer recognizer, ClassifierManager classifierManager)
        {
            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Debug("SyncRequest");

            var pipeline = client.CurrentPipeline = new Pipeline();

            var task1 = pipeline.StartNew(async () =>
            {
                try
                {
                    var recognition = await recognizer.SyncRecognition(
                       request.audio,
                       GoogleRecognizer.ConvertAudioEncoding(request.Config.encoding),
                       request.Config.sample_rate,
                       request.Config.language_code,
                        request.Config.context);

                    //
                    pipeline.SpeechResult = new SpeechResult(SpeechResult.Modes.ASR);
                    pipeline.SpeechResult.Transcript = recognition.Transcript;
                    pipeline.SpeechResult.Confidence = recognition.Confidence;
                }
                catch (Exception e)
                {
                    SendError(client, e);
                    pipeline.CancelExecution();
                }
            });

            var task2 = pipeline.StartNew(async () =>
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
                catch(Classification.Exception.BadModelException e)
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

            var task3 = pipeline.StartNew(async () =>
            {
                SendResult(client, pipeline.SpeechResult);
                pipeline = client.CurrentPipeline = null;
            });

            pipeline.WaitAll(task1, task2, task3);
        }
    }
}
