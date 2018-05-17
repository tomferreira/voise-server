using log4net;
using System;
using System.Threading.Tasks;
using Voise.Synthesizer.Exception;
using Voise.Synthesizer.Microsoft;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessSynthVoiceRequest : ProcessBase
    {
        private VoiseSynthVoiceRequest _request;
        private MicrosoftSynthetizer _synthetizer;

        internal ProcessSynthVoiceRequest(ClientConnection client, VoiseSynthVoiceRequest request, MicrosoftSynthetizer synthetizer)
            : base(client)
        {
            _request = request;
            _synthetizer = synthetizer;
        }

        // For while, its only implemented Microsoft Synthetizer
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
                var encoding = MicrosoftSynthetizer.ConvertAudioEncoding(_request.Config.encoding);

                int bytesPerSample = encoding != AudioEncoding.EncodingUnspecified ? encoding.BitsPerSample / 8 : 1;

                _client.StreamOut = new AudioStream(20, _request.Config.sample_rate, bytesPerSample);

                _client.StreamOut.DataAvailable += delegate (object sender, AudioStream.StreamInEventArgs e)
                {
                    VoiseResult result = new VoiseResult(VoiseResult.Modes.TTS);
                    result.AudioContent = Convert.ToBase64String(e.Buffer);

                    log.Debug($"Sending stream data ({e.Buffer.Length} bytes) at pipeline {_client.CurrentPipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

                    SendResult(result);
                };

                _synthetizer.Create(
                    _client.StreamOut,
                    encoding,
                    _request.Config.sample_rate,
                    _request.Config.language_code);

                SendAccept();

                pipeline.Result = new VoiseResult(VoiseResult.Modes.TTS);

                await _synthetizer.SynthAsync(_client.StreamOut, _request.text).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // Cleanup streamOut
                _client.StreamOut = null;

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

            // Cleanup streamOut
            _client.StreamOut = null;

            // Send end of stream
            pipeline.Result.AudioContent = string.Empty;
            SendResult(pipeline.Result);

            log.Info($"Request successful finished at pipeline {pipeline.Id}. [Client: {_client.RemoteEndPoint.ToString()}]");

            pipeline = _client.CurrentPipeline = null;
        }
    }
}
