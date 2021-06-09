using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voise.General;
using Voise.General.Interface;
using Voise.TCP.Response;

namespace Voise.TCP
{
    delegate void ClosedEventHandler(IClientConnection client);

    interface IClientConnection : IDisposable
    {
        event ClosedEventHandler Closed;

        IAudioStream StreamIn { get; set; }

        //
        IAudioStream StreamOut { get; set; }

        Pipeline CurrentPipeline { get; set; }

        System.Net.EndPoint RemoteEndPoint { get; }

        bool IsOpen();

        void SendResponse(VoiseResponse response);
    }
}
