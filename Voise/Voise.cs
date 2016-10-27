using log4net;
using log4net.Config;
using System;
using System.Threading;
using Voise.Classification;
using Voise.Process;
using Voise.Recognizer.Google;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise
{
    public class Voise
    {
        public static void Main(string[] args)
        {
            BasicConfigurator.Configure();

            try
            {
                Config config = new Config();
                new Voise(config);
            }
            catch(Exception e)
            {
                ILog log = LogManager.GetLogger(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

                log.Fatal(e.Message);
            }
        }

        private Server _tcpServer;
        private GoogleRecognizer _recognizer;
        private ClassifierManager _classifierManager;

        internal Voise(Config config)
        {
            _tcpServer = new Server(HandleClientRequest);
            _recognizer = new GoogleRecognizer();
            _classifierManager = new ClassifierManager(config.ClassifiersPath);

            if (config.TunningEnabled)
                _recognizer.EnableTunnig(config.TunningPath);

            _tcpServer.Start(config.Port);

            while (true)
                Thread.Sleep(100);
        }

        private void HandleClientRequest(ClientConnection client, VoiseRequest request)
        {
            if (request.SyncRequest != null)
            {
                new ProcessSyncRequest(
                    client, request.SyncRequest, _recognizer, _classifierManager);
            }
            else if (request.StreamStartRequest != null)
            {
                new ProcessStreamStartRequest(
                    client, request.StreamStartRequest, _recognizer, _classifierManager);
            }
            else if (request.StreamDataRequest != null)
            {
                new ProcessStreamDataRequest(
                    client, request.StreamDataRequest);
            }
            else if (request.StreamStopRequest != null)
            {
                new ProcessStreamStopRequest(
                    client, request.StreamStopRequest, _recognizer);
            }
        }
    }
}
