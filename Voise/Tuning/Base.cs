using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Voise.General;
using Voise.TCP.Request;

namespace Voise.Tuning
{
    public abstract class Base : IDisposable
    {
        public enum InputMethod
        {
            Sync,
            Stream
        };

        private bool _running;

        private readonly string _resultPath;
        private readonly MemoryStream _recording;
        private WaveFormat _waveFormat;

        protected Dictionary<string, string> _attrs;

        protected bool _shouldPersist;
        private bool _disposed;

        protected Base(string basePath, string direction, InputMethod inputMethod, VoiseConfig config)
        {
            _running = false;

            DateTime now = DateTime.Now;

            string fullPath = $"{basePath}/{now.ToString("yyyy-MM-dd")}/{direction}/";
            string filename = $"{now.ToString("HHmmss.fff")}-{new Random().Next()}";

            CreateDirectory(fullPath);

            _resultPath = $"{fullPath}/{filename}";
            _recording = new MemoryStream();

            _attrs = new Dictionary<string, string>
            {
                { "Input Method", inputMethod.ToString() },
                { "Engine ID", config.EngineID },
                { "Encoding", config.Encoding },
                { "Sample Rate", config.SampleRate.ToString() },
                { "Language Code", config.LanguageCode }
            };

            CreateWaveFormat(config.Encoding, config.SampleRate);

            _running = true;
            _shouldPersist = false;
        }

        private void CreateWaveFormat(string encoding, int sampleRate)
        {
            _waveFormat = null;

            switch (encoding.ToUpperInvariant())
            {
                case Constant.ENCODING_FLAC:
                    _waveFormat = new WaveFormat(sampleRate, Constant.CHANNEL_MONO);
                    break;

                case Constant.ENCODING_LINEAR16:
                    _waveFormat = new WaveFormat(sampleRate, 16, Constant.CHANNEL_MONO);
                    break;

                case Constant.ENCODING_ALAW:
                    _waveFormat = WaveFormat.CreateALawFormat(sampleRate, Constant.CHANNEL_MONO);
                    break;

                case Constant.ENCODING_MULAW:
                    _waveFormat = WaveFormat.CreateMuLawFormat(sampleRate, Constant.CHANNEL_MONO);
                    break;
            }
        }

        internal void WriteRecording(byte[] data, int offset, int count)
        {
            if (!_running)
                return;

            _recording.Write(data, offset, count);
        }

        public abstract void SetResult(VoiseResult result);

        internal void Close()
        {
            if (!_running)
                return;

            _recording.Close();

            if (_shouldPersist)
                PersistTuning();

            _running = false;
        }

        private void PersistTuning()
        {
            if (_waveFormat != null)
            {
                using (WaveFileWriter writer = new WaveFileWriter($"{_resultPath}.wav", _waveFormat))
                {
                    byte[] data = _recording.ToArray();
                    writer.Write(data, 0, data.Length);
                }
            }

            var content = _attrs.Select(x => x.Key + ": " + x.Value).ToArray();
            File.WriteAllLines($"{_resultPath}.txt", content);
        }

        private static void CreateDirectory(string fullPath)
        {
            string dirPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _recording.Dispose();

            _disposed = true;
        }
    }
}
