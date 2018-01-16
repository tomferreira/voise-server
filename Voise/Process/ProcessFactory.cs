
using Voise.Classification;
using Voise.Recognizer;
using Voise.Synthesizer.Microsoft;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessFactory
    {
        private RecognizerManager _recognizerManager;
        private MicrosoftSynthetizer _synthetizer;
        private ClassifierManager _classifierManager;

        internal ProcessFactory(RecognizerManager recognizerManager, MicrosoftSynthetizer synthetizer, ClassifierManager classifierManager)
        {
            _recognizerManager = recognizerManager;
            _synthetizer = synthetizer;
            _classifierManager = classifierManager;
        }

        internal ProcessBase createProcess(ClientConnection client, VoiseRequest request)
        {
            if (request.SyncRequest != null)
            {
                return new ProcessSyncRequest(
                    client, request.SyncRequest, _recognizerManager, _classifierManager);
            }

            if (request.StreamStartRequest != null)
            {
                return new ProcessStreamStartRequest(
                    client, request.StreamStartRequest, _recognizerManager, _classifierManager);
            }

            if (request.StreamDataRequest != null)
            {
                return new ProcessStreamDataRequest(
                    client, request.StreamDataRequest);
            }

            if (request.StreamStopRequest != null)
            {
                return new ProcessStreamStopRequest(
                    client, request.StreamStopRequest, _recognizerManager);
            }

            if (request.SynthVoiceRequest != null)
            {
                return new ProcessSynthVoiceRequest(
                    client, request.SynthVoiceRequest, _synthetizer);
            }

            return null;
        }
    }
}
