using System;
using System.IO;

namespace Voise.Tuning
{
    internal abstract class Base : IDisposable
    {
        internal enum InputMethod
        {
            Sync,
            Stream
        };

        protected FileStream _recording;
        protected string _resultPath;

        protected InputMethod _inputMethod;
        private bool _disposed;

        internal Base(string path, string direction, InputMethod inputMethod, string encoding, int sampleRate, string languageCode)
        {
            DateTime now = DateTime.Now;

            string fullPath = $"{path}\\{now.ToString("yyyy-MM-dd")}\\{direction}";
            string filename = $"{now.ToString("HHmmss")}-{encoding}-{sampleRate}-{languageCode}";

            CreateRecordingFile($"{fullPath}\\{filename}.wav");

            _resultPath = $"{fullPath}\\{filename}.txt";

            _inputMethod = inputMethod;
        }

        internal void WriteRecording(byte[] data, int offset, int count)
        {
            _recording.Write(data, offset, count);
        }

        private void CreateRecordingFile(string audioPath)
        {
            string dirPath = Path.GetDirectoryName(audioPath);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            _recording = File.Create(audioPath);
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
            {
                _recording.Close();
                _recording.Dispose();                
            }

            _disposed = true;
        }
    }
}
