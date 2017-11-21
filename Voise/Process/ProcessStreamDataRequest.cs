﻿using log4net;
using System;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessStreamDataRequest : ProcessBase
    {
        internal static async void Execute(ClientConnection client, VoiseStreamRecognitionDataRequest request)
        {
            ILog log = LogManager.GetLogger(typeof(ProcessStreamDataRequest));

            try
            {
                byte[] data = Convert.FromBase64String(request.data);

                log.Debug($"Receiving stream data ({data.Length} bytes) at pipeline {client.CurrentPipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

                client.StreamIn?.Write(data);
            }
            catch(Exception e)
            {
                log.Error($"{e.Message}\nStackTrace: {e.StackTrace}. [Client: {client.RemoteEndPoint().ToString()}]");
            }
        }
    }
}
