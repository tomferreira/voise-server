using log4net;
using log4net.Config;
using System;
using System.Threading;
using Voise.Classification;
using Voise.Process;
using Voise.Recognizer.Google;
using Voise.Synthesizer.Microsoft;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise
{
    public class Voise
    {
#if DEBUG
        public static void Main(string[] args)
        {
            BasicConfigurator.Configure();

            try
            {
                Config config = new Config();
                new Voise(config);

                while (true)
                    Thread.Sleep(100);
            }
            catch(Exception e)
            {
                ILog log = LogManager.GetLogger(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

                log.Fatal(e.Message);
            }
        }
#endif

        private Server _tcpServer;
        private GoogleRecognizer _recognizer;
        private MicrosoftSynthetizer _synthetizer;
        private ClassifierManager _classifierManager;

        public Voise(Config config)
        {
            _tcpServer = new Server(HandleClientRequest);

            // ASR
            _recognizer = new GoogleRecognizer();
            _classifierManager = new ClassifierManager(config.ClassifiersPath);

            // TTS
            _synthetizer = new MicrosoftSynthetizer();

            if (config.TunningEnabled)
                _recognizer.EnableTunnig(config.TunningPath);

            _tcpServer.Start(config.Port);
        }

        public void Stop()
        {
            if (_tcpServer != null && _tcpServer.IsOpen)
                _tcpServer.Stop();
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
            else if (request.SynthVoiceRequest != null)
            {
                new ProcessSynthVoiceRequest(client, request.SynthVoiceRequest, _synthetizer);
            }
        }
    }
}
