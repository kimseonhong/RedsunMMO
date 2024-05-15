﻿using RedsunLibrary.Network.Server;
using RedsunLibrary.Utils;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace RedsunLibrary.Network.UDP
{
	public class UDPSession : ISession
	{
		public SyncState<SocketRecvState_e> RecvState;
		public SyncState<SocketSendState_e> SendState;

		private UDPSession _listenerSession;
		private Int64 _sessionId;
		private EndPoint _endpoint;
		private RawSocket _socket;

		private SocketAsyncEventArgs _recvEventArgs;
		private SocketAsyncEventArgs _sendEventArgs;

		private byte[] _recvPacketBuffer;
		private byte[] _sendPacketBuffer;
		private ConcurrentQueue<(Packet, EndPoint)> _sendPackets;

		private UDPSessionManager _sessionManager;
		private IUDPSessionEventHandler _sessionEventHandler;

		public Int64 GetSessionId() => _sessionId;
		public EndPoint EndPoint => _endpoint;

		public UDPSession(UDPSession session, UDPSessionManager sessionManager, IUDPSessionEventHandler sessionEventHandler)
			: this(sessionEventHandler)
		{
			_sessionManager = sessionManager;

			_listenerSession = session;
		}

		public UDPSession(EndPoint endPoint, UDPSessionManager sessionManager, IUDPSessionEventHandler sessionEventHandler)
			: this(sessionEventHandler)
		{
			_sessionManager = sessionManager;

			_endpoint = endPoint;
			_recvEventArgs.RemoteEndPoint = _endpoint;
			_sendEventArgs.RemoteEndPoint = _endpoint;
		}

		public UDPSession(IUDPSessionEventHandler sessionEventHandler)
		{
			RecvState = new SyncState<SocketRecvState_e>(SocketRecvState_e.NOT_RECEIVING);
			SendState = new SyncState<SocketSendState_e>(SocketSendState_e.NOT_SENDING);

			_sessionId = 0;
			_socket = new RawSocket(ESocketType.UDP);

			_sessionEventHandler = sessionEventHandler;

			_recvPacketBuffer = new byte[PacketConst.TCP_RECV_BUFFER_SIZE];
			_sendPacketBuffer = new byte[PacketConst.TCP_SEND_BUFFER_SIZE];
			_sendPackets = new ConcurrentQueue<(Packet, EndPoint)>();

			_recvEventArgs = new SocketAsyncEventArgs();
			_recvEventArgs.UserToken = this;
			_recvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onReceiveCompleted);
			_recvEventArgs.SetBuffer(_recvPacketBuffer, 0, _recvPacketBuffer.Length);

			_sendEventArgs = new SocketAsyncEventArgs();
			_sendEventArgs.UserToken = this;
			_sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(onSendCompleted);
			_sendEventArgs.SetBuffer(_sendPacketBuffer, 0, _sendPacketBuffer.Length);
		}

		public void Bind()
		{
			_socket.Bind(_endpoint);
		}

		public void Connect(IPEndPoint endPoint)
		{
			_socket.Connect(endPoint);
		}

		public void AcceptSession(Int64 sessionId, EndPoint endPoint)
		{
			_sessionId = sessionId;
			_endpoint = endPoint;

			_recvEventArgs.RemoteEndPoint = _endpoint;
			_sendEventArgs.RemoteEndPoint = _endpoint;
		}

		public void ReceiveAsync(int pendingCount = 0)
		{
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
				Logger.Print(e.ToString());
			}

			if (false == pending)
			{
				ReceiveAsync(pendingCount++);
			}
		}

		private void onReceiveCompleted(object sender, SocketAsyncEventArgs e)
		{
			UDPSession client = this;
			try
			{
				// if Receive Size <= 0 , Socket Disconnect!1
				if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
				{
					// UDP는 데이터가 도착할때 Dgram 형식임으로 패킷 덩어리가 분실될지언정 도착했다면 모든 데이터가 온거임.
					if (_sessionManager != null)
					{
						client = _sessionManager.FindOrPopSession(e.RemoteEndPoint);
					}

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
					Close();
				}
			}
			catch (Exception ex)
			{
				_sessionEventHandler?.onInvaliedReceived(client, ex);
				Close();
			}
		}

		public void SendAsync(Packet packet, EndPoint endPoint)
		{
			// Not Sending 이 아니라면 전송중인상태임으로 큐에 패킷 쌓음
			if (false == SendState.IsState(SocketSendState_e.NOT_SENDING))
			{
				_sendPackets.Enqueue((packet, endPoint));
				return;
			}

			_SendAsync(packet, endPoint);
		}

		public void SendAsync(Packet packet)
		{
			if (_listenerSession != null)
			{
				_listenerSession.SendAsync(packet, EndPoint);
				return;
			}

			SendAsync(packet, EndPoint);
		}

		private void _SendAsync(Packet packet = null, EndPoint endPoint = null)
		{
			if (packet == null && endPoint == null)
			{
				if (false == _sendPackets.TryDequeue(out var data))
				{
					SendState.ExchangeNotEqual(SocketSendState_e.NOT_SENDING, out var old2State);
					return;
				}

				packet = data.Item1;
				endPoint = data.Item2;
			}

			// 여기는 packet 이 null 이 아님
			SendState.ExchangeNotEqual(SocketSendState_e.SENDING, out var oldState);
			// 복사 및 세팅
			var bytes = packet.PacketToByteArray();
			Buffer.BlockCopy(bytes, 0, _sendPacketBuffer, 0, bytes.Length);
			_sendEventArgs.SetBuffer(0, bytes.Length);
			_sendEventArgs.RemoteEndPoint = endPoint;

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
			_socket?.Close();
			Dispose();
		}

		public void Dispose()
		{
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