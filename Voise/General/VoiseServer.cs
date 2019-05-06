using log4net;
using System.Threading.Tasks;
using Voise.Classification;
using Voise.Process;
using Voise.Recognizer;
using Voise.Synthesizer;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.Tuning;

namespace Voise.General
{
    public class VoiseServer
    {
        private TCP.Server _tcpServer;
        private Config _config;
        private ProcessFactory _processFactory;

        private ILog _log;

        public VoiseServer(Config config)
        {
            _log = LogManager.GetLogger(typeof(VoiseServer));
            _log.Info($"Initializing Voise Server v{Version.VersionString}.");

            _config = config;

            _tcpServer = new TCP.Server(HandleClientRequest);

            // ASR
            RecognizerManager recognizerManager = new RecognizerManager(_config);
            ClassifierManager classifierManager = new ClassifierManager(_config);

            // TTS
            SynthesizerManager synthesizerManager = new SynthesizerManager(_config);

            //
            TuningManager tuningManager = new TuningManager(_config);

            _processFactory = new ProcessFactory(
                recognizerManager, synthesizerManager, classifierManager, tuningManager);
        }

        public void Start()
        {
            _tcpServer.StartAsync(_config.Port);

            _log.Info("Voise Server started.");
        }

        public void Stop()
        {
            if (_tcpServer != null && _tcpServer.IsOpen)
                _tcpServer.Stop();

            _log.Info("Voise Server stopped.");
        }

        private async Task HandleClientRequest(ClientConnection client, VoiseRequest request)
        {
            ProcessBase process = _processFactory.CreateProcess(client, request);

            if (process != null)
                await process.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
