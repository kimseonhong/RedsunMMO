using System;
using System.Net;
using System.Net.Sockets;

namespace RedsunLibrary.Network
{
	public class RawSocket : IDisposable
	{
		private object _lockObj;

		private Socket _socket;
		private IPAddress _ipAddress;
		private Int32 _port;

		public RawSocket()
		{
			_lockObj = new object();
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

		public RawSocket(Socket in_socket)
		{
			_lockObj = new object();
			_socket = in_socket;

			var endPoint = (IPEndPoint)_socket.RemoteEndPoint;
			_ipAddress = endPoint.Address;
			_port = endPoint.Port;
		}

		public void Initalize(IPAddress in_ipAddress, Int32 in_port)
		{
			_ipAddress = in_ipAddress;
			_port = in_port;

			_socket.NoDelay = true;
			//_socket.SendBufferSize = PacketConst.TCP_SEND_BUFFER_SIZE;
			//_socket.ReceiveBufferSize = PacketConst.TCP_RECV_BUFFER_SIZE;

			_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
		}

		public void Bind() => _socket.Bind(new IPEndPoint(_ipAddress, _port));

		public void Listen(Int32 in_backlog) => _socket.Listen(in_backlog);

		public bool AcceptAsync(SocketAsyncEventArgs in_event) => _socket.AcceptAsync(in_event);

		public bool ConnectAsync(SocketAsyncEventArgs in_evnet) => _socket.ConnectAsync(in_evnet);

		public bool DisconnectAsync(SocketAsyncEventArgs in_event) => _socket.DisconnectAsync(in_event);

		public bool ReceiveAsync(SocketAsyncEventArgs in_event) => _socket.ReceiveAsync(in_event);

		public bool SendAsync(SocketAsyncEventArgs in_evnet) => _socket.SendAsync(in_evnet);


		public void Close()
		{
			lock (_lockObj)
			{
				if (null == _socket)
				{
					return;
				}

				if (true == _disposed)
				{
					return;
				}
				Dispose();
			}
		}

		private bool _disposed = false;

		public void Dispose()
		{
			lock (_lockObj)
			{
				if (true == _disposed)
				{
					return;
				}

				_socket.Shutdown(SocketShutdown.Both);
				_socket.Close();
				_socket.Dispose();
				_socket = null;

				_disposed = true;
			}
			GC.SuppressFinalize(this);
		}
	}
}
