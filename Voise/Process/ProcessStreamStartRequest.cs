using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Voise.Classification.Interface;
using Voise.General;
using Voise.Recognizer;
using Voise.Recognizer.Exception;
using Voise.Recognizer.Interface;
using Voise.Recognizer.Provider.Common;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.Tuning;
using Voise.Tuning.Interface;

namespace Voise.Process
{
    internal class ProcessStreamStartRequest : ProcessBase
    {
        private readonly VoiseStreamRecognitionStartRequest _request;
        private readonly IRecognizerManager _recognizerManager;
        private readonly IClassifierManager _classifierManager;

        private readonly TuningIn _tuningIn;

        internal ProcessStreamStartRequest(IClientConnection client, VoiseStreamRecognitionStartRequest request,
            IRecognizerManager recognizerManager, IClassifierManager classifierManager, ITuningManager tuningManager)
            : base(client)
        {
            _request = request;
            _recognizerManager = recognizerManager;
            _classifierManager = classifierManager;

            _tuningIn = tuningManager.CreateTuningIn(TuningIn.InputMethod.Stream, _request.Config);
        }

        internal override async Task ExecuteAsync()
        {
            ILog log = LogManager.GetLogger(typeof(ProcessStreamStartRequest));

            // This client already is streaming audio.
            if (_client.StreamIn != null)
            {
                log.Warn($"Client already is streaming audio. [Client: {_client.RemoteEndPoint.ToString()}]");

                SendError(new Exception("Client already is streaming audio."));
                return;
            }

            var pipeline = _client.CurrentPipeline = new Pipeline();

            log.Info($"Starting stream request with engine '{_request.Config.engine_id}' at pipeline {pipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

            try
            {
                ICommonRecognizer recognizer = _recognizerManager.GetRecognizer(_request.Config.engine_id);

                // Set the recognizer for be used when to stop the stream
                _client.CurrentPipeline.Recognizer = recognizer;

                Dictionary<string, List<string>> contexts = GetContexts(_request.Config, _classifierManager);

                _client.StreamIn = new AudioStream(100, _request.Config.sample_rate, 2, _tuningIn);

                await recognizer.StartStreamingRecognitionAsync(
                    _client.StreamIn,
                    _request.Config.encoding,
                    _request.Config.sample_rate,
                    _request.Config.language_code,
                    contexts).ConfigureAwait(false);

                SendAccept();

                pipeline.Result = new VoiseResult(VoiseResult.Modes.ASR);
            }
            catch (Exception e)
            {
                // Cleanup streamIn
                _client.StreamIn?.Dispose();
                _client.StreamIn = null;

                _tuningIn?.Close();
                _tuningIn?.Dispose();

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

            // Espera pelo término do streaming para continuar a pipeline.
            // Veja o tratamento do comando 'StreamDataRequest'.
            await pipeline.WaitAsync().ConfigureAwait(false);

            // Caso ocorra alguma exceção assíncrona durante o streming do áudio
            if (pipeline.AsyncStreamError != null)
            {
                // Cleanup streamIn
                _client.StreamIn.Dispose();
                _client.StreamIn = null;

                _tuningIn?.Close();
                _tuningIn?.Dispose();

                log.Error($"{pipeline.AsyncStreamError.Message}. [Client: {_client.RemoteEndPoint.ToString()}]");

                SendError(pipeline.AsyncStreamError);
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
                            _request.Config.language_code,
                            pipeline.Result.Transcript).ConfigureAwait(false);

                        pipeline.Result.Intent = classification.ClassName;
                        pipeline.Result.Probability = classification.Probability;
                    }
                }

                _tuningIn?.SetResult(pipeline.Result);

                log.Info($"Stream request successful finished at pipeline {pipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

                SendResult(pipeline.Result);
            }
            catch (Exception e)
            {
                log.Error($"{e.Message}\nStacktrace: {e.StackTrace}. [Client: {_client.RemoteEndPoint.ToString()}]");

                SendError(e);
            }
            finally
            {
                // Cleanup streamIn
                _client.StreamIn.Dispose();
                _client.StreamIn = null;

                _tuningIn?.Close();
                _tuningIn?.Dispose();

                _client.CurrentPipeline.Dispose();
                _client.CurrentPipeline = null;
            }
        }
    }
}
