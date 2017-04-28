using System;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamDataRequest
    {
        internal ProcessStreamDataRequest(ClientConnection client, VoiseStreamRecognitionDataRequest request)
        {
            byte[] data = Convert.FromBase64String(request.data);
            client.StreamIn?.Write(data);
        }
    }
}
