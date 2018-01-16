using log4net;
using log4net.Config;
using System;
using System.Threading;
using System.Threading.Tasks;
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

                Voise voise = new Voise(config);
                voise.Start();

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
        private Config _config;
        private RecognizerManager _recognizerManager;
        private MicrosoftSynthetizer _synthetizer;
        private ClassifierManager _classifierManager;

        public Voise(Config config)
        {
            ILog log = LogManager.GetLogger(typeof(Voise));

            log.Info($"Initializing Voise Server v{Version.VersionString}.");

            _config = config;

            _tcpServer = new Server(HandleClientRequest);

            // ASR
            _recognizerManager = new RecognizerManager(_config);
            _classifierManager = new ClassifierManager(_config);

            // TTS
            _synthetizer = new MicrosoftSynthetizer();
        }

        public void Start()
        {
            _tcpServer.StartAsync(_config.Port);
        }

        public void Stop()
        {
            if (_tcpServer != null && _tcpServer.IsOpen)
                _tcpServer.Stop();
        }

        private async Task HandleClientRequest(ClientConnection client, VoiseRequest request)
        {
            ProcessBase process = null;

            if (request.SyncRequest != null)
            {
                process = new ProcessSyncRequest(
                    client, request.SyncRequest, _recognizerManager, _classifierManager);
            }
            else if (request.StreamStartRequest != null)
            {
                process = new ProcessStreamStartRequest(
                    client, request.StreamStartRequest, _recognizerManager, _classifierManager);
            }
            else if (request.StreamDataRequest != null)
            {
                process = new ProcessStreamDataRequest(
                    client, request.StreamDataRequest);
            }
            else if (request.StreamStopRequest != null)
            {
                process = new ProcessStreamStopRequest(
                    client, request.StreamStopRequest, _recognizerManager);
            }
            else if (request.SynthVoiceRequest != null)
            {
                process = new ProcessSynthVoiceRequest(
                    client, request.SynthVoiceRequest, _synthetizer);
            }

            if (process != null)
                await process.ExecuteAsync();
        }
    }
}
