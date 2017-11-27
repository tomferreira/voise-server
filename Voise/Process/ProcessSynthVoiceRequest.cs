using log4net;
using System;
using Voise.Synthesizer.Exception;
using Voise.Synthesizer.Microsoft;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    class ProcessSynthVoiceRequest : ProcessBase
    {
        // For while, its only implemented Microsoft Synthetizer
        internal static async void Execute(ClientConnection client, VoiseSynthVoiceRequest request, MicrosoftSynthetizer synthetizer)
        {
            ILog log = LogManager.GetLogger(typeof(ProcessSynthVoiceRequest));

            // This client already is receiving stream.
            if (client.StreamOut != null)
            {
                log.Error($"Client already is receiving stream. [Client: {client.RemoteEndPoint().ToString()}]");

                SendError(client, new Exception("Client already is receiving stream."));
                return;
            }

            var pipeline = client.CurrentPipeline = new Pipeline();

            log.Info($"Starting synth request at pipeline {pipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

            try
            {
                var encoding = MicrosoftSynthetizer.ConvertAudioEncoding(request.Config.encoding);

                int bytesPerSample = encoding != AudioEncoding.EncodingUnspecified ? encoding.BitsPerSample / 8 : 1;

                client.StreamOut = new AudioStream(20, request.Config.sample_rate, bytesPerSample);

                client.StreamOut.DataAvailable += delegate (object sender, AudioStream.StreamInEventArgs e)
                {
                    VoiseResult result = new VoiseResult(VoiseResult.Modes.TTS);
                    result.AudioContent = Convert.ToBase64String(e.Buffer);

                    log.Debug($"Sending stream data ({e.Buffer.Length} bytes) at pipeline {client.CurrentPipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

                    SendResult(client, result);
                };

                synthetizer.Create(
                    client.StreamOut, 
                    encoding,
                    request.Config.sample_rate,
                    request.Config.language_code);

                SendAccept(client);

                pipeline.Result = new VoiseResult(VoiseResult.Modes.TTS);

                synthetizer.Synth(client.StreamOut, request.text);
            }
            catch (Exception e)
            {
                // Cleanup streamOut
                client.StreamOut = null;

                if (e is BadEncodingException)
                {
                    log.Info($"{e.Message} [Client: {client.RemoteEndPoint().ToString()}]");
                }
                else
                {
                    log.Error($"{e.Message}\nStacktrace: {e.StackTrace}. [Client: {client.RemoteEndPoint().ToString()}]");
                }

                SendError(client, e);
                return;
            }

            // Cleanup streamOut
            client.StreamOut = null;

            // Send end of stream
            pipeline.Result.AudioContent = "";
            SendResult(client, pipeline.Result);

            log.Info($"Request successful finished at pipeline {pipeline.Id}. [Client: {client.RemoteEndPoint().ToString()}]");

            pipeline = client.CurrentPipeline = null;
        }
    }
}
