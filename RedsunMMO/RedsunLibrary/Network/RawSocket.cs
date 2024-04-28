using System;
using System.Net;
using System.Net.Sockets;

namespace RedsunLibrary.Network
{
	public enum ESocketType
	{
		_NONE = 0,

		TCP = 1,
		UDP = 2,

		_END
	}

	public class RawSocket : IDisposable
	{
		private object _lockObj;
		private ESocketType _socketType;

		private Socket _socket;
		private IPAddress _ipAddress;
		private Int32 _port;

		public RawSocket(ESocketType socketType)
		{
			_lockObj = new object();
			_socketType = socketType;

			if (socketType == ESocketType.TCP)
			{
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}
			else if (socketType == ESocketType.UDP)
			{
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			}
			else
			{
				throw new Exception("Invalid Socket Type");
			}
		}

		public RawSocket(ESocketType socketType, Socket socket)
		{
			_lockObj = new object();
			_socket = socket;
			_socketType = socketType;

			var endPoint = (IPEndPoint)_socket.RemoteEndPoint;
			_ipAddress = endPoint.Address;
			_port = endPoint.Port;
		}

		public void Initalize(IPAddress ipAddress, Int32 port)
		{
			_ipAddress = ipAddress;
			_port = port;

			if (_socketType == ESocketType.TCP)
			{
				_socket.NoDelay = true;

				//_socket.SendBufferSize = PacketConst.TCP_SEND_BUFFER_SIZE;
				//_socket.ReceiveBufferSize = PacketConst.TCP_RECV_BUFFER_SIZE;

				_socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
			}
		}

		public void Bind() => _socket.Bind(new IPEndPoint(_ipAddress, _port));
		public void Bind(EndPoint endPoint)
		{
			_ipAddress = ((IPEndPoint)endPoint).Address;
			_port = ((IPEndPoint)endPoint).Port;
			_socket.Bind(endPoint);
		}

		public void Listen(Int32 backlog) => _socket.Listen(backlog);

		public bool AcceptAsync(SocketAsyncEventArgs eventArgs) => _socket.AcceptAsync(eventArgs);

		public bool ConnectAsync(SocketAsyncEventArgs eventArgs) => _socket.ConnectAsync(eventArgs);
		public void Connect(EndPoint endPoint) => _socket.Connect(endPoint);

		public bool DisconnectAsync(SocketAsyncEventArgs eventArgs) => _socket.DisconnectAsync(eventArgs);

		public bool ReceiveAsync(SocketAsyncEventArgs eventArgs)
		{
			if (_socketType == ESocketType.TCP)
				return _socket.ReceiveAsync(eventArgs);
			else if (_socketType == ESocketType.UDP)
				return _socket.ReceiveFromAsync(eventArgs);
			else
				throw new Exception("Invalid Socket Type");
		}

		public bool SendAsync(SocketAsyncEventArgs eventArgs)
		{
			if (_socketType == ESocketType.TCP)
				return _socket.SendAsync(eventArgs);
			else if (_socketType == ESocketType.UDP)
				return _socket.SendToAsync(eventArgs);
			else
				throw new Exception("Invalid Socket Type");
		}


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
