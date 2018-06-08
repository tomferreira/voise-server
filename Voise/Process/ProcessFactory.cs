
using Voise.Classification;
using Voise.Recognizer;
using Voise.Synthesizer;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessFactory
    {
        private RecognizerManager _recognizerManager;
        private SynthetizerManager _synthetizerManager;
        private ClassifierManager _classifierManager;

        internal ProcessFactory(RecognizerManager recognizerManager, SynthetizerManager synthetizerManager, ClassifierManager classifierManager)
        {
            _recognizerManager = recognizerManager;
            _synthetizerManager = synthetizerManager;
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
                    client, request.SynthVoiceRequest, _synthetizerManager);
            }

            return null;
        }
    }
}
