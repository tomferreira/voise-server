using log4net;
using log4net.Config;
using System;
using System.Threading;
using Voise.Classification;
using Voise.Process;
using Voise.Recognizer;
using Voise.Synthesizer.Microsoft;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise
{
    public class Voise
    {
        public static void Main(string[] args)
        {
#if DEBUG
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

                log.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
            }
#else
            Console.WriteLine("To Start Voise Server, use the Windows Service.");
#endif
        }

        private Server _tcpServer;
        private RecognizerManager _recognizerManager;
        private MicrosoftSynthetizer _synthetizer;
        private ClassifierManager _classifierManager;

        public Voise(Config config)
        {
            ILog log = LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            log.Info($"Initializing Voise Server v{Version.VersionString}.");

            _tcpServer = new Server(HandleClientRequest);

            // ASR
            _recognizerManager = new RecognizerManager(config.RecognizersEnabled);
            _classifierManager = new ClassifierManager(config.ClassifiersPath);

            // TTS
            _synthetizer = new MicrosoftSynthetizer();                

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
                ProcessSyncRequest.Execute(
                    client, request.SyncRequest, _recognizerManager, _classifierManager);
            }
            else if (request.StreamStartRequest != null)
            {
                ProcessStreamStartRequest.Execute(
                    client, request.StreamStartRequest, _recognizerManager, _classifierManager);
            }
            else if (request.StreamDataRequest != null)
            {
                ProcessStreamDataRequest.Execute(
                    client, request.StreamDataRequest);
            }
            else if (request.StreamStopRequest != null)
            {
                ProcessStreamStopRequest.Execute(
                    client, request.StreamStopRequest, _recognizerManager);
            }
            else if (request.SynthVoiceRequest != null)
            {
                ProcessSynthVoiceRequest.Execute(
                    client, request.SynthVoiceRequest, _synthetizer);
            }
        }
    }
}
