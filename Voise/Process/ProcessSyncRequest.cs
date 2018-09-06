using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Classification;
using Voise.Recognizer;
using Voise.Recognizer.Exception;
using Voise.Recognizer.Provider.Common;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.Tuning;

namespace Voise.Process
{
    internal class ProcessSyncRequest : ProcessBase
    {
        private readonly VoiseSyncRecognitionRequest _request;
        private readonly RecognizerManager _recognizerManager;
        private readonly ClassifierManager _classifierManager;

        private TuningIn _tuning;

        internal ProcessSyncRequest(ClientConnection client, VoiseSyncRecognitionRequest request,
            RecognizerManager recognizerManager, ClassifierManager classifierManager, TuningManager tuningManager)
            : base(client)
        {
            _request = request;
            _recognizerManager = recognizerManager;
            _classifierManager = classifierManager;

            _tuning = tuningManager.CreateTuningIn(TuningIn.InputMethod.Sync, _request.Config);
        }

        internal override async Task ExecuteAsync()
        {
            ILog log = LogManager.GetLogger(typeof(ProcessStreamStartRequest));

            var pipeline = _client.CurrentPipeline = new Pipeline();

            log.Info($"Starting request with engine '{_request.Config.engine_id}' at pipeline {pipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

            try
            {
                CommonRecognizer recognizer = _recognizerManager.GetRecognizer(_request.Config.engine_id);

                var audio = Util.ConvertAudioToBytes(_request.audio);

                _tuning?.WriteRecording(audio, 0, audio.Length);

                Dictionary<string, List<string>> contexts = GetContexts(_request.Config, _classifierManager);

                var recognition = await recognizer.SyncRecognition(
                    audio,
                    _request.Config.encoding,
                    _request.Config.sample_rate,
                    _request.Config.language_code,
                    contexts).ConfigureAwait(false);

                //
                pipeline.Result = new VoiseResult(VoiseResult.Modes.ASR);
                pipeline.Result.Transcript = recognition.Transcript;
                pipeline.Result.Confidence = recognition.Confidence;
            }
            catch (Exception e)
            {
                if (e is BadEncodingException || e is BadAudioException)
                {
                    log.Info($"{e.Message} [Client: {_client.RemoteEndPoint.ToString()}]");
                }
                else
                {
                    log.Error($"{e.Message}\nStacktrace: {e.StackTrace}. [Client: {_client.RemoteEndPoint.ToString()}]");
                }

                SendError(e);
                return;
            }

            try
            {
                if (_request.Config.model_name != null && pipeline.Result.Transcript != null)
                {
                    if (pipeline.Result.Transcript == SpeechRecognitionResult.NoResult.Transcript)
                    {
                        pipeline.Result.Intent = SpeechRecognitionResult.NoResult.Transcript;
                        pipeline.Result.Probability = 1;
                    }
                    else
                    {
                        var classification = await _classifierManager.ClassifyAsync(
                            _request.Config.model_name,
                            pipeline.Result.Transcript).ConfigureAwait(false);

                        pipeline.Result.Intent = classification.ClassName;
                        pipeline.Result.Probability = classification.Probability;
                    }
                }

                log.Info($"Request successful finished at pipeline {pipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

                //
                _tuning?.SetResult(pipeline.Result);

                SendResult(pipeline.Result);
            }
            catch (Exception e)
            {
                log.Error($"{e.Message}\nStacktrace: {e.StackTrace}. [Client: {_client.RemoteEndPoint.ToString()}]");

                SendError(e);
            }
            finally
            {
                _tuning?.Close();
                _tuning?.Dispose();

                _client.CurrentPipeline.Dispose();
                _client.CurrentPipeline = null;
            }
        }
    }
}
