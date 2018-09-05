
using Voise.Classification;
using Voise.Recognizer;
using Voise.Synthesizer;
using Voise.TCP;
using Voise.TCP.Request;

namespace Voise.Process
{
    internal class ProcessFactory
    {
        private readonly RecognizerManager _recognizerManager;
        private readonly SynthesizerManager _synthesizerManager;
        private readonly ClassifierManager _classifierManager;

        internal ProcessFactory(RecognizerManager recognizerManager, SynthesizerManager synthesizerManager, ClassifierManager classifierManager)
        {
            _recognizerManager = recognizerManager;
            _synthesizerManager = synthesizerManager;
            _classifierManager = classifierManager;
        }

        internal ProcessBase CreateProcess(ClientConnection client, VoiseRequest request)
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
                    client, request.SynthVoiceRequest, _synthesizerManager);
            }

            return null;
        }
    }
}
