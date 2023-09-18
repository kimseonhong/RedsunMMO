using RedsunLibrary.Network;
using RedsunLibrary.Utils;
using System.Net;
using System.Net.Sockets;

namespace RedsunClient.Core.Network
{
	public class SimpleClientSocket
	{
		public Socket? ClientSocket => mSocket;

		public bool IsConnected => mSocket?.Connected ?? false;

		public string ConnectHost { get; private set; } = string.Empty;
		public ushort ConnectPort { get; private set; } = 0;


		public Action<string, ushort>? OnConnected { get; set; } = null;
		public Action<string, ushort>? OnDisconnected { get; set; } = null;
		public Action<string, ushort>? OnConnectFailed { get; set; } = null;
		public Action<Packet>? OnReceived { get; set; } = null;








		private Socket? mSocket = null;
		private bool mIsConnecting = false;

		private SocketAsyncEventArgs mRecvEvent = new SocketAsyncEventArgs();
		private SocketAsyncEventArgs mSendEvent = new SocketAsyncEventArgs();
		private SocketAsyncEventArgs mConnectEvent = new SocketAsyncEventArgs();

		private byte[] mRecvBuffer = new byte[PacketConst.MAX_PACKET_SIZE * 2];
		private byte[] mSendBuffer = new byte[PacketConst.MAX_PACKET_SIZE * 2];

		private object mSendLock = new object();
		private Queue<byte[]> mSendList = new Queue<byte[]>();

		private byte[] mPacketResolveBuffer = new byte[PacketConst.MAX_PACKET_SIZE];
		private int mTotalPacketSize = 0;
		private int mOffset = 0;



		public SimpleClientSocket()
		{
			_ResetBuffer();

			mRecvEvent.Completed += _RecvCompleted;
			mSendEvent.Completed += _SendCompleted;



			mConnectEvent.Completed += _ConnectCompleted;
		}


		public void Reset()
		{
			Disconnect();

		}

		public void Disconnect()
		{
			if (null == mSocket)
				return;

			if (!IsConnected)
				return;

			OnDisconnected?.Invoke(ConnectHost, ConnectPort);

			try
			{
				mSocket.Shutdown(SocketShutdown.Both);
			}
			catch (Exception)
			{
			}
			finally
			{
				mSocket.Close();
				mSocket = null;

				ConnectHost = string.Empty;
				ConnectPort = 0;
			}
			mIsConnecting = false;

			lock (mSendLock)
				mSendList.Clear();
		}

		public void Send(Packet msg)
		{
			// PacketToByteArray() 할때 모든것이 설정됨
			// 따라서 이후에 할 것.

			//if (!msg._IsValidHeader()
			//    || !msg._IsValidBody())
			//    return;

			lock (mSendLock)
			{
				bool bNeedToStart = mSendList.Count == 0;
				byte[] packet = msg.PacketToByteArray();
				if (packet == null)
				{
					// Todo : PacketToByteArray 의 return 이 null 이면 Compress 실패 또는 Compress 이후 Setbody 실패
					return;
				}
				mSendList.Enqueue(packet);
				if (bNeedToStart)
					_StartSend();
			}
		}



		public bool Connect(string host, int port)
		{
			if (IsConnected)
				return false;   // 이미 접속됨

			if (mIsConnecting)
				return false;   // 이미 접속중

			if (port <= 0
				|| port > ushort.MaxValue)
				return false;

			if (!IPAddress.TryParse(host, out var address))
			{
				try
				{
					var ip = Dns.GetHostAddresses(host);
					if (ip.Length == 0)
						return false;

					address = ip[0];
				}
				catch (Exception)
				{
					return false;
				}
			}

			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			mConnectEvent.RemoteEndPoint = new IPEndPoint(address, port);
			ConnectHost = host;
			ConnectPort = (ushort)port;
			_ResetBuffer();

			mIsConnecting = true;
			try
			{
				if (!mSocket.ConnectAsync(mConnectEvent))
					_ConnectCompleted(null, mConnectEvent);
			}
			catch (Exception)
			{
				Disconnect();
				return false;
			}

			return true;
		}


		private void _ResetBuffer()
		{
			mRecvEvent.SetBuffer(mRecvBuffer, 0, mRecvBuffer.Length);
			mSendEvent.SetBuffer(mSendBuffer, 0, mSendBuffer.Length);
		}



		private void _RecvProcess(SocketAsyncEventArgs e, int pendingCount = 0)
		{
			if (e.BytesTransferred == 0
				|| e.SocketError != SocketError.Success)
			{
				Disconnect();
				return;
			}

			_OnReceive(e.Buffer, e.Offset, e.BytesTransferred);

			if (null == mSocket)
				return;

			try
			{
				if (false == mSocket.ReceiveAsync(e))
					_RecvProcess(e, ++pendingCount);
			}
			catch (ObjectDisposedException)
			{
				// already socket closed
			}
			catch (Exception)
			{
				Disconnect();
			}
		}

		private void _SendProcess(SocketAsyncEventArgs e)
		{
			if (e.BytesTransferred <= 0
				|| e.SocketError != SocketError.Success)
				return;

			lock (mSendLock)
			{
				if (mSendList.Count == 0)
					return;

				var sendingPacket = mSendList.Peek();

				// Get
				var size = NetworkBitConverter.ToInt16(sendingPacket, PacketConst.PACKET_TOTAL_SIZE_OFFSET);
				if (e.BytesTransferred != size)
					return;

				mSendList.Dequeue();

				if (mSendList.Count > 0)
					_StartSend();

			}
		}

		private void _StartSend()
		{
			if (mSocket == null)
				return;

			if (!IsConnected)
				return;


			lock (mSendLock)
			{
				if (mSendList.Count == 0)
					return;

				byte[] sendPacket = mSendList.Peek();
				mSendEvent.SetBuffer(mSendEvent.Offset, sendPacket.Length);
				if (null == mSendEvent.Buffer)
					return; // 흠...?

				Buffer.BlockCopy(sendPacket, 0, mSendEvent.Buffer, mSendEvent.Offset, sendPacket.Length);
			}

			if (!mSocket.SendAsync(mSendEvent))
				_SendProcess(mSendEvent);
		}

		private void _OnReceive(byte[]? buffer, int offset, int bytesTransferred)
		{
			if (null == buffer
				|| offset < 0
				|| offset > buffer.Length
				|| bytesTransferred <= 0
				|| offset + bytesTransferred > buffer.Length)
				return;

			var DoRead = (int size) =>
			{
				Buffer.BlockCopy(buffer, offset, mPacketResolveBuffer, mOffset, size);

				bytesTransferred -= size;
				mOffset += size;
				offset += size;
			};

			var ResetResolve = () =>
			{
				mTotalPacketSize = 0;
				mOffset = 0;
			};

			int nReadSize;

			while (bytesTransferred > 0)
			{
				// 전체 크기를 아직 못구했을 경우
				if (mOffset < PacketConst.PACKET_TOTAL_SIZE) // 맨 앞 2바이트다
				{
					int nRemainSizeCheck = PacketConst.PACKET_TOTAL_SIZE - mOffset;
					nReadSize = nRemainSizeCheck;
					if (bytesTransferred < nReadSize)
						nReadSize = bytesTransferred;   // 읽을 데이터가 부족하면 어쩔수 없지. 읽을 수 있을만큼만

					DoRead(nReadSize);

					if (mOffset == PacketConst.PACKET_TOTAL_SIZE)
					{
						mTotalPacketSize = NetworkBitConverter.ToUInt16(mPacketResolveBuffer, PacketConst.PACKET_TOTAL_SIZE_OFFSET);
						if (mTotalPacketSize < PacketConst.MAX_PACKET_SIZE)
							ResetResolve();
					}
					continue;
				}

				// 사이즈만큼 체워주기
				nReadSize = Math.Min(bytesTransferred, mTotalPacketSize - mOffset);
				DoRead(nReadSize);

				if (mOffset == mTotalPacketSize)
				{
					Packet packet = new Packet();
					packet.ByteArrayToPacket(mPacketResolveBuffer, 0, mTotalPacketSize);
					OnReceived?.Invoke(packet);
					ResetResolve();
				}
			}
		}






		#region SocketAsyncEvent

		private void _SendCompleted(object? sender, SocketAsyncEventArgs e)
		{
			_SendProcess(e);
		}

		private void _RecvCompleted(object? sender, SocketAsyncEventArgs e)
		{
			if (e.LastOperation == SocketAsyncOperation.Receive)
				_RecvProcess(e);
		}
		private void _ConnectCompleted(object? sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError != SocketError.Success)
			{
				OnConnectFailed?.Invoke(ConnectHost, ConnectPort);
				Disconnect();
				return;
			}

			OnConnected?.Invoke(ConnectHost, ConnectPort);
			mIsConnecting = false;  // 접속됨
			if (null != mSocket)
			{
				if (!mSocket.ReceiveAsync(mRecvEvent))
					_RecvCompleted(null, mRecvEvent);
			}
		}

		#endregion
	}
}
