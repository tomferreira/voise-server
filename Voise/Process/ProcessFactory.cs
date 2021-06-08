using Voise.Classification.Interface;
using Voise.Recognizer.Interface;
using Voise.Synthesizer.Interface;
using Voise.TCP;
using Voise.TCP.Request;
using Voise.Tuning.Interface;

namespace Voise.Process
{
    internal class ProcessFactory
    {
        private readonly IRecognizerManager _recognizerManager;
        private readonly ISynthesizerManager _synthesizerManager;
        private readonly IClassifierManager _classifierManager;
        private readonly ITuningManager _tuningManager;

        internal ProcessFactory(
            IRecognizerManager recognizerManager, 
            ISynthesizerManager synthesizerManager, 
            IClassifierManager classifierManager, 
            ITuningManager tuningManager)
        {
            _recognizerManager = recognizerManager;
            _synthesizerManager = synthesizerManager;
            _classifierManager = classifierManager;
            _tuningManager = tuningManager;
        }

        internal ProcessBase CreateProcess(IClientConnection client, VoiseRequest request)
        {
            if (request.SyncRequest != null)
            {
                return new ProcessSyncRequest(
                    client, request.SyncRequest, _recognizerManager, _classifierManager, _tuningManager);
            }

            if (request.StreamStartRequest != null)
            {
                return new ProcessStreamStartRequest(
                    client, request.StreamStartRequest, _recognizerManager, _classifierManager, _tuningManager);
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
                    client, request.SynthVoiceRequest, _synthesizerManager, _tuningManager);
            }

            return null;
        }
    }
}
