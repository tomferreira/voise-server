using log4net;
using System;
using System.Threading.Tasks;
using Voise.General;
using Voise.General.Interface;
using Voise.Synthesizer.Exception;
using Voise.Synthesizer.Interface;
using Voise.Synthesizer.Provider.Common;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.Tuning;
using Voise.Tuning.Interface;

namespace Voise.Process
{
    internal class ProcessSynthVoiceRequest : ProcessBase
    {
        private readonly VoiseSynthVoiceRequest _request;
        private readonly ISynthesizerManager _synthesizerManager;

        private readonly TuningOut _tuningOut;

        internal ProcessSynthVoiceRequest(IClientConnection client, VoiseSynthVoiceRequest request,
            ISynthesizerManager synthesizerManager, ITuningManager tuningManager)
            : base(client)
        {
            _request = request;
            _synthesizerManager = synthesizerManager;

            _tuningOut = tuningManager.CreateTuningOut(TuningIn.InputMethod.Sync, _request.text, _request.Config);
        }

        // For while, its only implemented Microsoft Synthesizer
        internal override async Task ExecuteAsync()
        {
            ILog log = LogManager.GetLogger(typeof(ProcessSynthVoiceRequest));

            // This client already is receiving stream.
            if (_client.StreamOut != null)
            {
                log.Warn($"Client already is receiving stream. [Client: {_client.RemoteEndPoint.ToString()}]");

                SendError(new Exception("Client already is receiving stream."));
                return;
            }

            var pipeline = _client.CurrentPipeline = new Pipeline();

            log.Info($"Starting synth request at pipeline {pipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

            try
            {
                ICommonSynthesizer synthesizer = _synthesizerManager.GetSynthesizer(_request.Config.engine_id);

                int bytesPerSample = synthesizer.GetBytesPerSample(_request.Config.encoding);
                _client.StreamOut = new AudioStream(
                    _request.Config.max_frame_ms ?? 20, _request.Config.sample_rate, bytesPerSample, _tuningOut);

                _client.StreamOut.DataAvailable += delegate (object sender, IStreamInEventArgs e)
                {
                    VoiseResult result = new VoiseResult(VoiseResult.Modes.TTS)
                    {
                        AudioContent = Convert.ToBase64String(e.Buffer)
                    };

                    log.Debug($"Sending stream data ({e.Buffer.Length} bytes) at pipeline {_client.CurrentPipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

                    SendResult(result);
                };

                var job = synthesizer.SetSynth(
                    _client.StreamOut,
                    _request.Config.encoding,
                    _request.Config.sample_rate,
                    _request.Config.language_code);

                SendAccept();

                await synthesizer.SynthAsync(job, _request.text).ConfigureAwait(false);

                pipeline.Result = new VoiseResult(VoiseResult.Modes.TTS);

                _tuningOut?.SetResult(pipeline.Result);
            }
            catch (Exception e)
            {
                if (e is BadEncodingException)
                {
                    log.Info($"{e.Message} [Client: {_client.RemoteEndPoint.ToString()}]");
                }
                else
                {
                    log.Error($"{e.Message}\nStacktrace: {e.StackTrace}. [Client: {_client.RemoteEndPoint.ToString()}]");
                }

                SendError(e);
                return;
            }
            finally
            {
                // Cleanup streamOut
                _client.StreamOut.Dispose();
                _client.StreamOut = null;

                _tuningOut?.Close();
                _tuningOut?.Dispose();
            }

            // Send end of stream (Empty audio indicates end of stream)
            pipeline.Result.AudioContent = string.Empty;
            SendResult(pipeline.Result);

            log.Info($"Request successful finished at pipeline {pipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

            _client.CurrentPipeline.Dispose();
            _client.CurrentPipeline = null;
        }
    }
}
