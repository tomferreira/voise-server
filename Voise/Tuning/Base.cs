using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Voise.TCP.Request;

namespace Voise.Tuning
{
    internal abstract class Base : IDisposable
    {
        internal enum InputMethod
        {
            Sync,
            Stream
        };

        private bool _running;

        private string _resultPath;
        private MemoryStream _recording;
        private WaveFormat _waveFormat;

        protected Dictionary<string, string> _attrs;

        private InputMethod _inputMethod;
        protected bool _shouldPersist;
        private bool _disposed;

        internal Base(string basePath, string direction, InputMethod inputMethod, VoiseConfig config)
        {
            _running = false;

            DateTime now = DateTime.Now;

            _inputMethod = inputMethod;

            string fullPath = $"{basePath}/{now.ToString("yyyy-MM-dd")}/{direction}/";
            string filename = $"{now.ToString("HHmmss.fff")}-{new Random().Next()}";

            CreateDirectory(fullPath);

            _resultPath = $"{fullPath}/{filename}";
            _recording = new MemoryStream();

            _attrs = new Dictionary<string, string>();
            _attrs.Add("Input Method", _inputMethod.ToString());
            _attrs.Add("Engine ID", config.engine_id);
            _attrs.Add("Encoding", config.encoding);
            _attrs.Add("Sample Rate", config.sample_rate.ToString());
            _attrs.Add("Language Code", config.language_code);

            CreateWaveFormat(config.encoding, config.sample_rate);

            _running = true;
            _shouldPersist = false;
        }

        private void CreateWaveFormat(string encoding, int sampleRate)
        {
            _waveFormat = null;

            switch (encoding.ToLower())
            {
                case "flac":
                case "linear16":
                    _waveFormat = new WaveFormat(sampleRate, 1);
                    break;

                case "alaw":
                    _waveFormat = WaveFormat.CreateALawFormat(sampleRate, 1);
                    break;

                case "mulaw":
                    _waveFormat = WaveFormat.CreateMuLawFormat(sampleRate, 1);
                    break;
            }
        }

        internal void WriteRecording(byte[] data, int offset, int count)
        {
            if (!_running)
                return;

            _recording.Write(data, offset, count);
        }

        internal abstract void SetResult(VoiseResult result);

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

        private void CreateDirectory(string fullPath)
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
