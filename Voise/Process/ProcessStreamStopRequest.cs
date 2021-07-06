using log4net;
using System;
using System.Threading.Tasks;
using Voise.Recognizer.Interface;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamStopRequest : ProcessBase
    {
        private readonly VoiseStreamRecognitionStopRequest _request;

        internal ProcessStreamStopRequest(IClientConnection client, VoiseStreamRecognitionStopRequest request)
            : base(client)
        {
            _request = request;
        }

        internal override async Task ExecuteAsync()
        {
            ILog log = LogManager.GetLogger(typeof(ProcessStreamStopRequest));

            log.Info($"Stopping stream request at pipeline {_client.CurrentPipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

            try
            {
                if (_client.CurrentPipeline.Recognizer == null)
                    throw new Exception("Engine not defined.");

                var recognition =
                    await _client.CurrentPipeline.Recognizer.StopStreamingRecognitionAsync(_client.StreamIn).ConfigureAwait(false);

                _client.CurrentPipeline.Result.Transcript = recognition.Transcript;
                _client.CurrentPipeline.Result.Confidence = recognition.Confidence;
            }
            catch (Exception e)
            {
                Exception deepestException = e.InnerException ?? e;

                _client.CurrentPipeline.AsyncStreamError = deepestException;
                log.Error($"{deepestException?.Message} [Client: {_client.RemoteEndPoint.ToString()}]");
            }

            // Avisa à pipeline que pode continuar.
            _client.CurrentPipeline.ReleaseMutex();
        }
    }
}
