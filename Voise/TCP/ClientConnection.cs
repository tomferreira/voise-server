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

        private Socket _socket;
        private SocketAsyncEventArgs _readEventArgs;

        private byte[] _buffer;
        private StringBuilder _data;

        private HandlerRequest _hr;

        private ILog _log;

        internal delegate void ClosedEventHandler(ClientConnection client);

        //
        internal AudioStream StreamIn { get; set; }

        //
        internal AudioStream StreamOut { get; set; }

        internal Pipeline CurrentPipeline { get; set; }

        internal ClientConnection(Socket acceptedSocket, HandlerRequest hr)
        {
            _log = LogManager.GetLogger(
                    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            _log.Debug($"Initializing connection from {acceptedSocket.RemoteEndPoint.ToString()}.");

            _buffer = new byte[1024]; // 1 KB
            _data = new StringBuilder();

            _socket = acceptedSocket;
            _hr = hr;

            _readEventArgs = new SocketAsyncEventArgs();
            _readEventArgs.Completed += SockAsyncEventArgs_Completed;
            _readEventArgs.SetBuffer(_buffer, 0, _buffer.Length);
            _readEventArgs.UserToken = this;

            ReceiveAsync(_readEventArgs);
        }

        internal event ClosedEventHandler Closed;

        internal bool IsOpen()
        {
            lock (_socket)
                return _socket.Connected;
        }

        internal System.Net.EndPoint RemoteEndPoint()
        {
            lock (_socket)
                return _socket.RemoteEndPoint;
        }

        private void CloseConnection()
        {
            lock (_socket)
            {
                _buffer = null;

                _data.Clear();
                _data = null;

                _readEventArgs.Completed -= SockAsyncEventArgs_Completed;
                _readEventArgs.SetBuffer(null, 0, 0);
                _readEventArgs.Dispose();

                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception) { }

                _socket.Close();
            }

            _log = null;

            // If stream yet is started, abort it.
            if (StreamIn != null && StreamIn.IsStarted())
                StreamIn.Abort();

            // If stream yet is started, abort it.
            if (StreamOut != null && StreamOut.IsStarted())
                StreamOut.Abort();

            Closed?.Invoke(this);
        }

        private void SockAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
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

            _socket.Send(byteData);
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
