using log4net;
using System;
using System.Threading.Tasks;
using Voise.Synthesizer;
using Voise.Synthesizer.Exception;
using Voise.Synthesizer.Provider.Common;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.Tuning;

namespace Voise.Process
{
    internal class ProcessSynthVoiceRequest : ProcessBase
    {
        private readonly VoiseSynthVoiceRequest _request;
        private readonly SynthesizerManager _synthesizerManager;

        private TuningOut _tuning;

        internal ProcessSynthVoiceRequest(ClientConnection client, VoiseSynthVoiceRequest request, 
            SynthesizerManager synthesizerManager, TuningManager tuningManager)
            : base(client)
        {
            _request = request;
            _synthesizerManager = synthesizerManager;

            _tuning = tuningManager.CreateTuningOut(TuningIn.InputMethod.Sync, _request.text, _request.Config);
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
                CommonSynthesizer synthesizer = _synthesizerManager.GetSynthesizer(_request.Config.engine_id);

                //var encoding = MicrosoftSynthesizer.ConvertAudioEncoding(_request.Config.encoding);
                //int bytesPerSample = encoding != AudioEncoding.EncodingUnspecified ? encoding.BitsPerSample / 8 : 1;
                int bytesPerSample = 2;

                _client.StreamOut = new AudioStream(20, _request.Config.sample_rate, bytesPerSample, _tuning);

                _client.StreamOut.DataAvailable += delegate (object sender, AudioStream.StreamInEventArgs e)
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

                _tuning?.SetResult(pipeline.Result);
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

                _tuning?.Close();
                _tuning?.Dispose();
            }

            // Send end of stream
            pipeline.Result.AudioContent = string.Empty;
            SendResult(pipeline.Result);

            log.Info($"Request successful finished at pipeline {pipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

            _client.CurrentPipeline.Dispose();
            _client.CurrentPipeline = null;
        }
    }
}
