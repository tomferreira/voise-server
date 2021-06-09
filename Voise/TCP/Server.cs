using log4net;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Voise.TCP.Request;

namespace Voise.TCP
{
    internal class Server
    {
        internal delegate Task HandlerRequest(IClientConnection client, VoiseRequest request);

        private Socket _listenSocket;
        private SocketAsyncEventArgs _acceptAsyncArgs;
        private List<IClientConnection> _connections;
        private readonly HandlerRequest _hr;

        private ILog _log;

        internal Server(HandlerRequest hr)
        {
            _log = LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            _hr = hr;
            _connections = new List<IClientConnection>();
        }

        internal bool IsOpen { get; private set; }

        internal async Task StartAsync(int port)
        {
            await Task.Run(() =>
            {
                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                _acceptAsyncArgs = new SocketAsyncEventArgs();
                _acceptAsyncArgs.Completed += AcceptCompleted;

                _listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                _listenSocket.Listen(50);

                Accept(_acceptAsyncArgs);

                IsOpen = true;
            }).ConfigureAwait(false);
        }

        internal async Task StopAsync()
        {
            await Task.Run(() =>
            {
                IsOpen = false;

                _listenSocket.Close();
                _listenSocket = null;

                lock (_connections)
                    _connections.Clear();
            }).ConfigureAwait(false);
        }

        private void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (IsOpen)
            {
                if (e.SocketError == SocketError.Success)
                {
                    IClientConnection client = new ClientConnection(e.AcceptSocket, _hr);
                    client.Closed += ClientClosed;

                    lock (_connections)
                        _connections.Add(client);
                }

                e.AcceptSocket = null;

                Accept(_acceptAsyncArgs);
            }
        }

        private void Accept(SocketAsyncEventArgs e)
        {
            bool willRaiseEvent = _listenSocket.AcceptAsync(e);

            if (!willRaiseEvent)
                AcceptCompleted(_listenSocket, e);
        }

        private void ClientClosed(IClientConnection client)
        {
            client.Closed -= ClientClosed;

            ClearClientConnection(client);
        }

        private void ClearClientConnection(IClientConnection client)
        {
            client.Dispose();

            lock (_connections)
            {
                // Remove closed client
                _connections.Remove(client);

                _log.Debug($"There are still {_connections.Count} active connections.");
            }
        }
    }
}
