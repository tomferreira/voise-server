using CPqDAsr.ASR;
using CPqDASR.Config;
using System;
using System.Threading;
using System.Threading.Tasks;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Cpqd.Job
{
    internal class StreamingJob : Base, IStreamingJob
    {
        private readonly AudioStream _streamIn;

        public StreamingJob(ClientConfig config, AudioStream streamIn, LanguageModelList modelList, AudioEncoding encoding, int sampleRate, string languageCode)
            : base(config, modelList)
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
            _streamIn.StreamingStopped += StreamingStopped;
        }

        public Task StartAsync()
        {
            return Task.Run(() =>
            {
                _audioSource = new Internal.BufferAudioSource();

                _speechRecognizer.Recognize(_audioSource, _modelList);

                _streamIn.Start();
            });
        }

        public Task StopAsync()
        {
            return Task.Run(() =>
            {
                _streamIn.Stop();

                lock (_monitorCompleted)
                {
                    if (!_completed)
                        Monitor.Wait(_monitorCompleted);
                }
            });
        }

        private void ConsumeStreamData(object sender, AudioStream.StreamInEventArgs e)
        {
            _audioSource.Write(e.Buffer);
        }

        private void StreamingStopped(object sender, EventArgs e)
        {
            _audioSource.Finish();
        }
    }
}
