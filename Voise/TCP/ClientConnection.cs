using log4net;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using Voise.TCP.Request;
using Voise.TCP.Response;
using static Voise.TCP.Server;

namespace Voise.TCP
{
    internal class ClientConnection
    {
        private const string DELIMITER = "<EOF>";
        private static int _clientNumberID = 0;

        private Socket _socket;
        private SocketAsyncEventArgs _readEventArgs;
        private SocketAsyncEventArgs _writeEventArgs;

        private byte[] _buffer;
        private StringBuilder _data;
        private HandlerRequest _hr;

        private ILog _log;

        internal int ClientNumber { get; private set; }
        internal StreamIn StreamIn { get; set; }
        internal Pipeline CurrentPipeline { get; set; }

        internal ClientConnection(Socket acceptedSocket, HandlerRequest hr)
        {
            _log = LogManager.GetLogger(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            ClientNumber = _clientNumberID++;

            _log.Info($"Initializing connection from {acceptedSocket.RemoteEndPoint.ToString()}.");

            _buffer = new byte[1024]; // 1 KB
            _data = new StringBuilder();

            _socket = acceptedSocket;
            _hr = hr;

            _readEventArgs = new SocketAsyncEventArgs();
            _readEventArgs.Completed += SockAsyncEventArgs_Completed;
            _readEventArgs.SetBuffer(_buffer, 0, _buffer.Length);
            _readEventArgs.UserToken = this;

            _writeEventArgs = new SocketAsyncEventArgs();
            _writeEventArgs.Completed += SockAsyncEventArgs_Completed;
            _writeEventArgs.UserToken = this;

            ReceiveAsync(_readEventArgs);
        }

        internal bool IsOpen()
        {
            lock (_socket)
                return _socket.Connected;
        }

        private void CloseConnection()
        {
            lock (_socket)
            {
                _data = null;

                _readEventArgs.Dispose();
                _writeEventArgs.Dispose();

                try
                {
                    _socket.Shutdown(SocketShutdown.Send);
                }
                catch (Exception) { }

                _socket.Close();
            }
        }

        private void SockAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
            }
        }

        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                CloseConnection();
                return;
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred <= 0)
            {
                CloseConnection();
                return;
            }

            lock (_data)
            {
                _data.Append(Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred));

                for (int index = 0; index > -1;)
                {
                    index = _data.ToString().IndexOf(DELIMITER);

                    if (index > -1)
                    {
                        string content = _data.ToString(0, index);

                        _data.Remove(0, index + DELIMITER.Length);

                        VoiseRequest request = JsonConvert.DeserializeObject<VoiseRequest>(content);

                        HandleRequest(request);
                    }
                };
            }

            ReceiveAsync(e);
        }

        internal void SendResponse(VoiseResponse response)
        {
            if (response == null)
                return;

            string data = JsonConvert.SerializeObject(response) + DELIMITER;

            // Convert the string data to byte data using UTF8 encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            _writeEventArgs.SetBuffer(byteData, 0, byteData.Length);

            SendAsync(_writeEventArgs);
        }

        private void SendAsync(SocketAsyncEventArgs e)
        {
            bool willRaiseEvent = _socket.SendAsync(e);

            if (!willRaiseEvent)
                ProcessSend(e);
        }

        private void ReceiveAsync(SocketAsyncEventArgs e)
        {
            bool willRaiseEvent = _socket.ReceiveAsync(e);

            if (!willRaiseEvent)
                ProcessReceive(e);
        }

        private void HandleRequest(VoiseRequest request)
        {
            _hr?.Invoke(this, request);
        }
    }
}
