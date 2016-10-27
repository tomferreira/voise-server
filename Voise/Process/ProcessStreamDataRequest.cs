using log4net;
using System;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamDataRequest
    {
        internal ProcessStreamDataRequest(ClientConnection client, VoiseStreamRecognitionDataRequest request)
        {
            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Debug("StreamDataRequest");

            byte[] data = Convert.FromBase64String(request.data);
            client.StreamIn?.AddData(data);
        }
    }
}
