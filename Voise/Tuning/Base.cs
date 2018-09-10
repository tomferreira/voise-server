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

        protected Dictionary<string, string> _data;

        private InputMethod _inputMethod;
        private bool _disposed;

        internal Base(string basePath, string direction, InputMethod inputMethod)
        {
            _running = false;

            DateTime now = DateTime.Now;

            _inputMethod = inputMethod;

            string fullPath = $"{basePath}/{now.ToString("yyyy-MM-dd")}/{direction}/";
            string filename = $"{now.ToString("HHmmss.fff")}-{new Random().Next()}";

            CreateDirectory(fullPath);

            _resultPath = $"{fullPath}/{filename}";
            _recording = new MemoryStream();

            _data = new Dictionary<string, string>();
            _data.Add($"Input Method", _inputMethod.ToString());

            _running = true;
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
            File.WriteAllBytes($"{_resultPath}.wav", _recording.ToArray());

            var content = _data.Select(x => x.Key + ": " + x.Value).ToArray();
            File.WriteAllLines($"{_resultPath}.txt", content);

            _running = false;
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
