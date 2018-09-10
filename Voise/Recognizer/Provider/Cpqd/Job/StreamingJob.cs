using CPqDAsr.ASR;
using CPqDASR.Config;
using log4net;
using System;
using System.Threading;
using System.Threading.Tasks;
using Voise.General;
using Voise.Recognizer.Provider.Common.Job;

namespace Voise.Recognizer.Provider.Cpqd.Job
{
    internal class StreamingJob : Base, IStreamingJob
    {
        private readonly AudioStream _streamIn;

        public StreamingJob(ClientConfig config, AudioStream streamIn, LanguageModelList modelList, AudioEncoding encoding, int sampleRate, string languageCode)
            : base(LogManager.GetLogger(typeof(StreamingJob)), config, modelList)
        {
            ValidateArguments(encoding, sampleRate, languageCode);

            _streamIn = streamIn;
            _streamIn.DataAvailable += ConsumeStreamData;
            _streamIn.StreamingStopped += StreamingStopped;
        }

        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                _audioSource = new Internal.BufferAudioSource();

                _speechRecognizer.Recognize(_audioSource, _modelList);

                _streamIn.Start();
            }).ConfigureAwait(false);
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
