using RedsunLibrary.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace RedsunLibrary.Network.TCP
{
	public class TCPServerListener
	{
		public const int DEFAULT_POOL_SIZE = 5000;
		private RawSocket _acceptSocket;
		private SocketAsyncEventArgs _acceptArgsEvent;

		private TCPSessionManager _sessionManager;
		private ITCPSessionEventHandler _sessionEventHandler;

		public TCPServerListener(ITCPSessionEventHandler sessionEventHandler)
		{
			_sessionEventHandler = sessionEventHandler;
		}

		public void StartListener(IPAddress ipAddress, Int32 port, Int32 backLog = 0)
		{
			_acceptSocket = new RawSocket(ESocketType.TCP);
			_acceptSocket.Initalize(ipAddress, port);
			_acceptSocket.Bind();
			_acceptSocket.Listen(backLog);

			_acceptArgsEvent = new SocketAsyncEventArgs();
			_acceptArgsEvent.Completed += new EventHandler<SocketAsyncEventArgs>(onAcceptAsyncCompleted);

			_sessionManager = new TCPSessionManager(_sessionEventHandler);
			_sessionManager.Initalize(DEFAULT_POOL_SIZE);

			_AcceptAsync();
		}

		public void StopListener()
		{
			_acceptArgsEvent.Dispose();
			_acceptArgsEvent = null;

			_sessionManager.Dispose();
			_sessionManager = null;
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
				TCPSession session = _sessionManager.PopSession(e.AcceptSocket);
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
