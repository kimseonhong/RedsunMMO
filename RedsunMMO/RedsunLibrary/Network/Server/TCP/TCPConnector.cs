using RedsunLibrary.Network.Server;
using RedsunLibrary.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RedsunLibrary.Network.TCP
{
	public class TCPConnector : ISession
	{
		public SyncState<SocketState_e> SocketState;
		public SyncState<SocketRecvState_e> RecvState;
		public SyncState<SocketSendState_e> SendState;

		private Int64 _sessionId;
		private RawSocket _socket;
		private PacketProcessor _packetProcessor;

		private SocketAsyncEventArgs _connectEventArgs;
		private SocketAsyncEventArgs _recvEventArgs;
		private SocketAsyncEventArgs _sendEventArgs;
		private SocketAsyncEventArgs _disconnectEventArgs;

		private ITCPConnectorEventHandler _sessionEventHandler;

		private byte[] _recvPacketBuffer;
		private byte[] _sendPacketBuffer;
		private ConcurrentQueue<Packet> _sendPackets;

		public Int64 GetSessionId() => _sessionId;

		public TCPConnector(ITCPConnectorEventHandler sessionEventHandler)
		{
			SocketState = new SyncState<SocketState_e>(SocketState_e.NOT_CONNECTED);
			RecvState = new SyncState<SocketRecvState_e>(SocketRecvState_e.NOT_RECEIVING);
			SendState = new SyncState<SocketSendState_e>(SocketSendState_e.NOT_SENDING);

			_sessionId = 0;
			_socket = new RawSocket(ESocketType.TCP);
			_packetProcessor = new PacketProcessor();

			_connectEventArgs = new SocketAsyncEventArgs();
			_connectEventArgs.UserToken = this;
			_connectEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onConnectCompleted);

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

			_disconnectEventArgs = new SocketAsyncEventArgs();
			_disconnectEventArgs.UserToken = this;
			_disconnectEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onDisconnectCompleted);

			_sessionEventHandler = sessionEventHandler;
		}

		public void Connect(EndPoint endPoint)
		{
			if (false == SocketState.IsState(SocketState_e.NOT_CONNECTED))
			{
				Logger.Print("Alreay Connected");
				_sessionEventHandler?.onConnectFailed("Alreay Connected");
				DisconnectAsync();
				return;
			}

			bool pending = true;
			try
			{
				_connectEventArgs.RemoteEndPoint = endPoint;
				SocketState.ExchangeNotEqual(SocketState_e.CONNECTING, out _);
				pending = _socket.ConnectAsync(_connectEventArgs);
			}
			catch (Exception e)
			{
				Logger.Print(e.ToString());
			}

			if (false == pending)
			{
				onConnectCompleted(this, _connectEventArgs);
			}
		}

		public void onConnectCompleted(object sender, SocketAsyncEventArgs e)
		{
			if (null == e)
			{
				SocketState.ExchangeNotEqual(SocketState_e.NOT_CONNECTED, out _);
				return;
			}

			if (SocketError.Success == e.SocketError)
			{
				SocketState.ExchangeNotEqual(SocketState_e.CONNECTED, out _);
				RecvState.Exchange(SocketRecvState_e.RECEIVING);
				ReceiveAsync();

				_sessionEventHandler?.onConnected();
			}
			else
			{
				_sessionEventHandler?.onConnectFailed($"{e.SocketError}");
			}
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
				onReceiveCompleted(_socket, _recvEventArgs);
			}
		}

		private void onReceiveCompleted(object sender, SocketAsyncEventArgs e)
		{
			try
			{
				// if Receive Size <= 0 , Socket Disconnect!1
				if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
				{
					_packetProcessor.ReceiveProcess(e.Buffer, e.BytesTransferred);

					// 패킷 꺼내와야지.
					while (true)
					{
						Packet packet = _packetProcessor.TakePacket();
						if (packet == null)
						{
							break;
						}

						_sessionEventHandler?.onReceived(packet);
					}

					ReceiveAsync();
					return;
				}
				else
				{
					DisconnectAsync();
				}
			}
			catch (Exception ex)
			{
				_sessionEventHandler?.onInvaliedReceived(ex);
				onDisconnectCompleted(this, _disconnectEventArgs);
			}
		}

		public void Send(Packet packet)
		{
			// Connected 가 아니라고? 
			if (false == SocketState.IsState(SocketState_e.CONNECTED))
			{
				DisconnectAsync();
				return;
			}

			// Not Sending 이 아니라면 전송중인상태임으로 큐에 패킷 쌓음
			if (false == SendState.IsState(SocketSendState_e.NOT_SENDING))
			{
				_sendPackets.Enqueue(packet);
				return;
			}

			_Send(packet);
		}

		private void _Send(Packet packet = null)
		{
			if (false == SocketState.IsState(SocketState_e.CONNECTED))
			{
				// 문제가 있음
				// 커넥트 상태가 아니라고..?
				DisconnectAsync();
				return;
			}

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
				DisconnectAsync();
			}
		}

		private void onSendCompleted(object sender, SocketAsyncEventArgs e)
		{
			try
			{
				// if Receive Size <= 0 , Socket Disconnect!1
				if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
				{
					_Send();
					return;
				}
				else
				{
					DisconnectAsync();
				}
			}
			catch (Exception ex)
			{
				_sessionEventHandler?.onInvaliedSent(ex);
				onDisconnectCompleted(this, _disconnectEventArgs);
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
			_socket.DisconnectAsync(_disconnectEventArgs);
		}

		private void onDisconnectCompleted(object sender, SocketAsyncEventArgs e)
		{
			SocketState.Exchange(SocketState_e.DISCONNECTED);
			_sessionEventHandler?.onDisconnected();
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
}
