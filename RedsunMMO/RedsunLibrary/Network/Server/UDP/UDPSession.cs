using RedsunLibrary.Network.Server;
using RedsunLibrary.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RedsunLibrary.Network.UDP
{
	public class UDPSession : ISession
	{
		public SyncState<SocketRecvState_e> RecvState;
		public SyncState<SocketSendState_e> SendState;

		private Int64 _sessionId;
		private EndPoint _endpoint;
		private RawSocket _socket;

		private SocketAsyncEventArgs _recvEventArgs;
		private SocketAsyncEventArgs _sendEventArgs;
		private byte[] _recvPacketBuffer;
		private byte[] _sendPacketBuffer;

		private ConcurrentQueue<Packet> _sendPackets;

		private UDPSessionManager _sessionManager;
		private IUDPSessionEventHandler _sessionEventHandler;

		public Int64 GetSessionId() => _sessionId;
		public EndPoint EndPoint => _endpoint;

		// 클라용
		public UDPSession(IUDPSessionEventHandler sessionEventHandler)
		{
			RecvState = new SyncState<SocketRecvState_e>(SocketRecvState_e.NOT_RECEIVING);
			SendState = new SyncState<SocketSendState_e>(SocketSendState_e.NOT_SENDING);

			_sessionId = 0;
			_socket = new RawSocket(ESocketType.UDP);

			_sessionEventHandler = sessionEventHandler;

			_recvPacketBuffer = new byte[PacketConst.TCP_RECV_BUFFER_SIZE];
			_sendPacketBuffer = new byte[PacketConst.TCP_SEND_BUFFER_SIZE];
			_sendPackets = new ConcurrentQueue<Packet>();

			_recvEventArgs = new SocketAsyncEventArgs();
			_recvEventArgs.UserToken = this;
			_recvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onReceiveCompleted);
			_recvEventArgs.SetBuffer(_recvPacketBuffer, 0, _recvPacketBuffer.Length);

			_sendEventArgs = new SocketAsyncEventArgs();
			_sendEventArgs.UserToken = this;
			_sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onSendCompleted);
			_sendEventArgs.SetBuffer(_sendPacketBuffer, 0, _sendPacketBuffer.Length);
		}

		// 서버내 세션용
		public UDPSession(UDPSession session, UDPSessionManager sessionManager, IUDPSessionEventHandler sessionEventHandler)
		{
			_sessionManager = sessionManager;
			_sessionEventHandler = sessionEventHandler;

			SendState = new SyncState<SocketSendState_e>(SocketSendState_e.NOT_SENDING);

			_socket = session._socket;

			_sendPacketBuffer = new byte[PacketConst.TCP_SEND_BUFFER_SIZE];
			_sendPackets = new ConcurrentQueue<Packet>();

			_sendEventArgs = new SocketAsyncEventArgs();
			_sendEventArgs.UserToken = this;
			_sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onSendCompleted);
			_sendEventArgs.SetBuffer(_sendPacketBuffer, 0, _sendPacketBuffer.Length);
		}

		// 서버용
		public UDPSession(EndPoint endPoint, UDPSessionManager sessionManager, IUDPSessionEventHandler sessionEventHandler)
		{
			_sessionManager = sessionManager;

			RecvState = new SyncState<SocketRecvState_e>(SocketRecvState_e.NOT_RECEIVING);

			_sessionId = 0;
			_socket = new RawSocket(ESocketType.UDP);

			_sessionEventHandler = sessionEventHandler;

			_recvPacketBuffer = new byte[PacketConst.TCP_RECV_BUFFER_SIZE];

			_recvEventArgs = new SocketAsyncEventArgs();
			_recvEventArgs.UserToken = this;
			_recvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onReceiveCompleted);
			_recvEventArgs.SetBuffer(_recvPacketBuffer, 0, _recvPacketBuffer.Length);

			_endpoint = endPoint;
			_recvEventArgs.RemoteEndPoint = _endpoint;
		}

		public void Bind()
		{
			_socket.Bind(_endpoint);
		}

		public void Connect(string host, Int32 port)
		{
			IPAddress[] addresses = Dns.GetHostAddresses(host);

			_endpoint = new IPEndPoint(addresses[0], port);
			_socket.Connect((IPEndPoint)_endpoint);

			_recvEventArgs.RemoteEndPoint = _endpoint;
			_sendEventArgs.RemoteEndPoint = _endpoint;

			ReceiveAsync();
		}

		public void AcceptSession(Int64 sessionId, EndPoint endPoint)
		{
			_sessionId = sessionId;
			_endpoint = endPoint;

			_sendEventArgs.RemoteEndPoint = endPoint;
		}

		public void ReceiveAsync(int pendingCount = 0)
		{
			if (_isDisposed) return;

			RecvState.Exchange(SocketRecvState_e.RECEIVING);
			// OverFlow 조심
			if (pendingCount > 5)
			{
				// 강제 종료시켜버리자고
				RecvState.Exchange(SocketRecvState_e.NOT_RECEIVING);
				Close();
				return;
			}

			bool pending = true;
			try
			{
				pending = _socket.ReceiveAsync(_recvEventArgs);
			}
			catch (Exception e)
			{
				Logger.PrintError(e.ToString());
			}

			if (false == pending)
			{
				onReceiveCompleted(_socket, _recvEventArgs);
			}
		}

		private void onReceiveCompleted(object sender, SocketAsyncEventArgs e)
		{
			if (_isDisposed) return;

			UDPSession client = this;
			if (_sessionManager != null)
			{
				client = _sessionManager.FindOrPopSession(e.RemoteEndPoint);
				if (client == null)
				{
					ReceiveAsync();
					return;
				}
			}

			try
			{
				// if Receive Size <= 0 , Socket Disconnect!1
				if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
				{
					// UDP는 데이터가 도착할때 Dgram 형식임으로 패킷 덩어리가 분실될지언정 도착했다면 모든 데이터가 온거임.
					var memory = new Memory<byte>(e.Buffer);
					var byteData = memory.Slice(0, e.BytesTransferred).ToArray();

					Packet packet = new Packet();
					if (false == packet.ByteArrayToPacket(byteData, 0, byteData.Length))
					{
						return;
					}

					_sessionEventHandler?.onReceived(client, packet);
					ReceiveAsync();
					return;
				}
				else
				{
					Close(client);

					if (_sessionManager != null)
						ReceiveAsync();
				}
			}
			catch (Exception ex)
			{
				_sessionEventHandler?.onInvaliedReceived(client, ex);
				Close(client);

				if (_sessionManager != null)
					ReceiveAsync();
			}
		}

		public void Send(Packet packet)
		{
			if (_isDisposed) return;

			// Not Sending 이 아니라면 전송중인상태임으로 큐에 패킷 쌓음
			if (false == SendState.IsState(SocketSendState_e.NOT_SENDING))
			{
				_sendPackets.Enqueue(packet);
				return;
			}

			_SendAsync(packet);
		}

		private void _SendAsync(Packet packet = null)
		{
			if (_isDisposed) return;

			if (packet == null)
			{
				if (false == _sendPackets.TryDequeue(out packet))
				{
					SendState.ExchangeNotEqual(SocketSendState_e.NOT_SENDING, out var old2State);
					return;
				}
			}

			// 여기는 packet 이 null 이 아님
			SendState.ExchangeNotEqual(SocketSendState_e.SENDING, out var oldState);
			// 복사 및 세팅
			var bytes = packet.PacketToByteArray();
			Buffer.BlockCopy(bytes, 0, _sendPacketBuffer, 0, bytes.Length);
			_sendEventArgs.SetBuffer(0, bytes.Length);

			try
			{
				bool pending = _socket.SendAsync(_sendEventArgs);
				if (pending == false)
				{
					onSendCompleted(this, _sendEventArgs);
				}
			}
			catch (Exception e)
			{
				Close();
			}
		}

		private void onSendCompleted(object sender, SocketAsyncEventArgs e)
		{
			if (_isDisposed) return;

			try
			{
				// if Receive Size <= 0 , Socket Disconnect!1
				if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
				{
					_SendAsync();
					return;
				}
				else
				{
					Close();
				}
			}
			catch (Exception ex)
			{
				_sessionEventHandler?.onInvaliedSent(this, ex);
				Close();
			}
		}

		public void Close()
		{
			if (_sessionId != 0)
			{
				_sessionManager?.PushSession(this);
				return;
			}

			_socket?.Close();
			Dispose();
		}

		public void Close(UDPSession session)
		{
			if (_sessionId != 0)
			{
				_sessionManager?.PushSession(session);
				return;
			}

			_socket?.Close();
			Dispose();
		}

		private bool _isDisposed = false;
		public void Dispose()
		{
			if (_isDisposed == true)
			{
				return;
			}

			_socket?.Dispose();
			_socket = null;

			RecvState = null;
			SendState = null;

			_recvEventArgs?.Dispose();
			_sendEventArgs?.Dispose();

			_recvEventArgs = null;
			_sendEventArgs = null;

			_isDisposed = true;
		}

		public void SessionDispose()
		{
			_isDisposed = true;
		}
	}
}