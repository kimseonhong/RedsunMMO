using RedsunLibrary.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace RedsunLibrary.Network.Server
{
    public class ServerListener
    {
        private RawSocket _acceptSocket;
        private SocketAsyncEventArgs _acceptArgsEvent;

        public ISessionEventHandler _sessionEventHandler;

        public void StartListener(IPAddress in_ipAddress, Int32 in_port, Int32 in_backLog = 0)
        {
            _acceptSocket = new RawSocket();
            _acceptSocket.Initalize(in_ipAddress, in_port);
            _acceptSocket.Bind();
            _acceptSocket.Listen(in_backLog);

            _acceptArgsEvent = new SocketAsyncEventArgs();
            _acceptArgsEvent.Completed += new EventHandler<SocketAsyncEventArgs>(onAcceptAsyncCompleted);

            _AcceptAsync();
        }

        private void _AcceptAsync()
        {
            if (null == _acceptArgsEvent)
            {
                return;
            }

            _acceptArgsEvent.AcceptSocket = null;

            bool pending = true;
            try
            {
                pending = _acceptSocket.AcceptAsync(_acceptArgsEvent);
            }
            catch (Exception e)
            {
                Logger.Print(e.ToString());
                _AcceptAsync();
            }

            if (!pending)
            {
                onAcceptAsyncCompleted(this, _acceptArgsEvent);
            }
        }

        private void onAcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (null == e)
            {
                return;
            }

            if (SocketError.Success == e.SocketError)
            {
                Session tcpSession = TcpSessionManager.it.CreateTcpSession(e.AcceptSocket);
                tcpSession.onAcceptAsyncProcess(m_simplePacketReceiver.SimplePacketMessageQueue);

                onAcceptAsyncProcess(tcpSession);
                _AcceptAsync();
                return;
            }
            else
            {
                //todo:Accept 실패 처리.
                Logger.Print("Failed to accept client. " + e.SocketError);
            }
            _AcceptAsync();
        }
    }
}
