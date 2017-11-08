using System;
using Voise.Synthesizer.Microsoft;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    class ProcessSynthVoiceRequest : ProcessBase
    {
        internal static async void Execute(ClientConnection client, VoiseSynthVoiceRequest request, MicrosoftSynthetizer synthetizer)
        {
            // This client already is receiving stream.
            if (client.StreamOut != null)
            {
                SendError(client, new Exception("Client already is receiving stream."));
                return;
            }

            var pipeline = client.CurrentPipeline = new Pipeline();

            try
            {
                var encoding = MicrosoftSynthetizer.ConvertAudioEncoding(request.Config.encoding);

                int bytesPerSample = encoding != AudioEncoding.EncodingUnspecified ? encoding.BitsPerSample / 8 : 1;

                client.StreamOut = new AudioStream(20, request.Config.sample_rate, bytesPerSample);

                client.StreamOut.DataAvailable += delegate (object sender, AudioStream.StreamInEventArgs e)
                {
                    SpeechResult result = new SpeechResult(SpeechResult.Modes.TTS);
                    result.AudioContent = Convert.ToBase64String(e.Buffer);
                    SendResult(client, result);
                };

                synthetizer.Create(
                    client.StreamOut, 
                    encoding,
                    request.Config.sample_rate,
                    request.Config.language_code);

                SendAccept(client);

                pipeline.SpeechResult = new SpeechResult(SpeechResult.Modes.TTS);
            }
            catch (Exception e)
            {
                // Cleanup streamOut
                client.StreamOut = null;

                SendError(client, e);
                return;
            }

            try
            { 
                synthetizer.Synth(client.StreamOut, request.text);
            }
            catch (Exception e)
            {
                // Cleanup streamOut
                client.StreamOut = null;

                SendError(client, e);
                return;
            }

            // Cleanup streamOut
            client.StreamOut = null;

            // End
            pipeline.SpeechResult.AudioContent = "";

            SendResult(client, pipeline.SpeechResult);
            pipeline = client.CurrentPipeline = null;
        }
    }
}
