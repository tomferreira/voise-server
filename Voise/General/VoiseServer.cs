using log4net;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Voise.Classification.Interface;
using Voise.General.Interface;
using Voise.Process;
using Voise.Recognizer.Interface;
using Voise.Synthesizer.Interface;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.Tuning.Interface;

namespace Voise.General
{
    public class VoiseServer : IHostedService
    {
        private TCP.Server _tcpServer;
        private IConfig _config;
        private ProcessFactory _processFactory;

        private ILog _logger;

        public VoiseServer(
            IConfig config,
            IRecognizerManager recognizerManager,
            IClassifierManager classifierManager,
            ISynthesizerManager synthesizerManager,
            ITuningManager tuningManager,
            ILog logger)
        {
            _logger = logger;
            _logger.Info($"Initializing Voise Server v{Version.VersionString}.");

            _config = config;

            _tcpServer = new TCP.Server(HandleClientRequest);

            _processFactory = new ProcessFactory(
                recognizerManager, synthesizerManager, classifierManager, tuningManager);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _tcpServer.StartAsync(_config.Port);

            _logger.Info("Voise Server started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_tcpServer != null && _tcpServer.IsOpen)
            {
                await _tcpServer.StopAsync();
            }

            _logger.Info("Voise Server stopped.");
        }

        private async Task HandleClientRequest(IClientConnection client, VoiseRequest request)
        {
            ProcessBase process = _processFactory.CreateProcess(client, request);

            if (process != null)
                await process.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
