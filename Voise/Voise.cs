using log4net;
using log4net.Config;
using System;
using System.Threading;
using System.Threading.Tasks;
using Voise.Classification;
using Voise.Process;
using Voise.Recognizer;
using Voise.Synthesizer;
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
            catch (Exception e)
            {
                ILog log = LogManager.GetLogger(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

                if (e is AggregateException)
                {
                    foreach (var ie in (e as AggregateException).InnerExceptions)
                        log.Fatal($"{ie.Message}\nStacktrace: {ie.StackTrace}");
                }
                else
                {
                    log.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
                }
            }
#else
            Console.WriteLine("To Start Voise Server, use the Windows Service.");
#endif
        }

        private Server _tcpServer;
        private Config _config;
        private ProcessFactory _processFactory;

        public Voise(Config config)
        {
            ILog log = LogManager.GetLogger(typeof(Voise));

            log.Info($"Initializing Voise Server v{Version.VersionString}.");

            _config = config;

            _tcpServer = new Server(HandleClientRequest);

            // ASR
            RecognizerManager recognizerManager = new RecognizerManager(_config);
            ClassifierManager classifierManager = new ClassifierManager(_config);

            // TTS
            SynthesizerManager synthesizerManager = new SynthesizerManager(_config);

            _processFactory = new ProcessFactory(
                recognizerManager, synthesizerManager, classifierManager);
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
            ProcessBase process = _processFactory.createProcess(client, request);

            if (process != null)
                await process.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
