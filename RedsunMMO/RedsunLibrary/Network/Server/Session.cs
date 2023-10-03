using RedsunLibrary.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace RedsunLibrary.Network.Server
{
    public class Session : IDisposable
    {
        public SyncState<SocketState_e> SocketState;
        public SyncState<SocketRecvState_e> RecvState;
        public SyncState<SocketSendState_e> SendState;

        private RawSocket _socket;

        private SocketAsyncEventArgs _recvEventArgs;
        private SocketAsyncEventArgs _sendEventArgs;

        private byte[] _recvPacketBuffer = new byte[PacketConst.TCP_RECV_BUFFER_SIZE];
        private byte[] _sendPacketBuffer = new byte[PacketConst.TCP_SEND_BUFFER_SIZE];

        public Session()
        {
            _socket = new RawSocket();
            SocketState = new SyncState<SocketState_e>(SocketState_e.NOT_CONNECTED);
            RecvState = new SyncState<SocketRecvState_e>(SocketRecvState_e.NOT_RECEIVING);
            SendState = new SyncState<SocketSendState_e>(SocketSendState_e.NOT_SENDING);
        }

        public void AcceptAsyncProcess()
        {
            _recvEventArgs.UserToken = this;
            _recvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onReceiveCompleted);
            _recvEventArgs.SetBuffer(_recvPacketBuffer, 0, _recvPacketBuffer.Length);

            _sendEventArgs.UserToken = this;
            _sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onSendCompleted);
            _sendEventArgs.SetBuffer(_sendPacketBuffer, 0, _sendPacketBuffer.Length);

            if (false == SocketState.ExchangeNotEqual(SocketState_e.CONNECTED, out var out_oldState))
            {
                Logger.Print("Alreay Connected Session");
            }

            //m_recvPacketMessageQueue = in_recvPacketMessageQueue;
            //ReceiveAsync();
            ReceiveAsync();
            RecvState.Exchange(SocketRecvState_e.RECEIVING);
        }

        public void ReceiveAsync(int pendingCount = 0)
        {
            if (false == SocketState.IsState(SocketState_e.CONNECTED))
            {
                // 커넥트 상태가 아니라고..?
                RecvState.Exchange(SocketRecvState_e.NOT_RECEIVING);
                return;
            }

            // OverFlow 조심
            if (pendingCount > 5)
            {
                // 강제 종료시켜버리자고
                RecvState.Exchange(SocketRecvState_e.NOT_RECEIVING);
                DisconnectAsync();
                return;
            }

            bool pending = true;
            try
            {
                pending = _socket.ReceiveAsync(_recvEventArgs);
            }
            catch (Exception e)
            {
                Logger.Print(e.ToString());
            }

            if (false == pending)
            {
                ReceiveAsync(pendingCount++);
            }
        }

        private void onReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
            }
            else
            {
            }
        }

        public void SendAsync()
        {
            if (false == SocketState.IsState(SocketState_e.CONNECTED))
            {
                // 문제가 있음
                // 커넥트 상태가 아니라고..?
                SendState.Exchange(SocketSendState_e.NOT_SENDING);
                return;
            }

        }

        private void onSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {

            }
            else
            {
            }
        }

        public void DisconnectAsync()
        {
            if (SocketState.IsState(SocketState_e.DISCONNECTED)
                || SocketState.IsState(SocketState_e.DISCONNECTING))
            {
                return;
            }

            SocketState.Exchange(SocketState_e.DISCONNECTING);

            var disConnectArgsEvent = new SocketAsyncEventArgs();
            disConnectArgsEvent.UserToken = this;
            disConnectArgsEvent.Completed += new EventHandler<SocketAsyncEventArgs>(onDisconnectCompleted);
        }

        private void onDisconnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            SocketState.Exchange(SocketState_e.DISCONNECTED);

            //SimpleTcpSocket tcpSocket = (SimpleTcpSocket)e.UserToken;
            //onDisConnectProcess?.Invoke(tcpSocket, m_disconnectResult);
        }

        public void Close()
        {
            _socket?.Close();
            Dispose();
        }

        public void Dispose()
        {
            SocketState = null;
            RecvState = null;
            SendState = null;

            _recvEventArgs?.Dispose();
            _sendEventArgs?.Dispose();

            _recvEventArgs = null;
            _sendEventArgs = null;

            _socket?.Dispose();
            _socket = null;
        }
    }

    public class SessionIdAllocate
    {
        // 1조
        private static Int64 m_defaultId = 1000000000000;
        private static Int64 m_currentAllocId = 0;

        public static void Init()
        {
            m_currentAllocId = 0;
        }
        public static Int64 AllocSessionId()
        {
            m_currentAllocId += 1;

            return m_defaultId + m_currentAllocId;
        }
    }

    public class SessionManager : SingleTon<SessionManager>
    {
        private Dictionary<Int64 /* SessionId */, Session> _sessionList;
        private Queue<Session> _sessionQueue;

        private Int64 _sessionId = 0;

        public SessionManager()
        {
            _sessionList = new Dictionary<Int64, Session>();
            _sessionQueue = new Queue<Session>();
            SessionIdAllocate.Init();
        }

        public void Initalize(int poolSize)
        {
            for (int i = 0; i < poolSize; i++)
            {
                Session session = new Session();
                _sessionQueue.Enqueue(session);
            }
        }
    }
}
