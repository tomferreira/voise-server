using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Voise.TCP.Request;

namespace Voise.TCP
{
    internal class Server
    {
        internal delegate void HandlerRequest(ClientConnection client, VoiseRequest request);

        private Socket _listenSocket;
        private SocketAsyncEventArgs _acceptAsyncArgs;
        private List<ClientConnection> _listConnection;
        private HandlerRequest _hr;

        internal Server(HandlerRequest hr)
        {
            _hr = hr;
            _listConnection = new List<ClientConnection>();            
        }

        internal bool IsOpen { get; private set; }

        internal void Start(int port)
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _acceptAsyncArgs = new SocketAsyncEventArgs();
            _acceptAsyncArgs.Completed += AcceptCompleted;

            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            _listenSocket.Listen(50);

            AcceptAsync(_acceptAsyncArgs);

            IsOpen = true;
        }

        internal void Stop()
        {
            IsOpen = false;

            _listenSocket.Close();            
            _listenSocket = null;

            lock (_listConnection)
                _listConnection.Clear();
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (IsOpen)
            {
                if (e.SocketError == SocketError.Success)
                {
                    ClientConnection Client = new ClientConnection(e.AcceptSocket, _hr);

                    lock (_listConnection)
                        _listConnection.Add(Client);
                }

                CheckClientConnections();

                e.AcceptSocket = null;

                AcceptAsync(_acceptAsyncArgs);
            }
        }

        private void AcceptAsync(SocketAsyncEventArgs e)
        {
            bool willRaiseEvent = _listenSocket.AcceptAsync(e);

            if (!willRaiseEvent)
                AcceptCompleted(_listenSocket, e);
        }

        private void CheckClientConnections()
        {
            lock (_listConnection)
            {
                // Remove closed connections
                _listConnection.RemoveAll(client => !client.IsOpen());
            }
        }
    }
}
