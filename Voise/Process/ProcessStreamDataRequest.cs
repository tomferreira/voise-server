using log4net;
using System;
using System.Threading.Tasks;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamDataRequest : ProcessBase
    {
        private VoiseStreamRecognitionDataRequest _request;

        internal ProcessStreamDataRequest(ClientConnection client, VoiseStreamRecognitionDataRequest request)
            : base(client)
        {
            _request = request;
        }

        internal override async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                ILog log = LogManager.GetLogger(typeof(ProcessStreamDataRequest));

                try
                {
                    byte[] data = Convert.FromBase64String(_request.data);

                    log.Debug($"Receiving stream data ({data.Length} bytes) at pipeline {_client.CurrentPipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

                    _client.StreamIn?.Write(data);
                }
                catch (Exception e)
                {
                    log.Error($"{e.Message}\nStackTrace: {e.StackTrace}. [Client: {_client.RemoteEndPoint.ToString()}]");
                }
            }).ConfigureAwait(false);
        }
    }
}
