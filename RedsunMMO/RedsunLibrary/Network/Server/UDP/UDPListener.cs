using System;
using System.Net;

namespace RedsunLibrary.Network.UDP
{
	public class UDPListener
	{
		public const int DEFAULT_POOL_SIZE = 2000;
		private UDPSession _session;
		private EndPoint _endPoint;

		private UDPSessionManager _sessionManager;
		private IUDPSessionEventHandler _sessionEventHandler;

		public UDPListener(IUDPSessionEventHandler sessionEventHandler)
		{
			_sessionEventHandler = sessionEventHandler;
		}

		public void StartListener(IPAddress address, Int32 port)
		{
			_endPoint = new IPEndPoint(address, port);

			_session = new UDPSession(_endPoint, _sessionManager, _sessionEventHandler);
			_session.Bind();

			_sessionManager = new UDPSessionManager(_sessionEventHandler);
			_sessionManager.Initalize(DEFAULT_POOL_SIZE);

			// 시작
			_session.ReceiveAsync();
		}

		public void Connect(string host, Int32 port)
		{
			IPAddress[] addresses = Dns.GetHostAddresses(host);
			_session.Connect(new IPEndPoint(addresses[0], port));
		}

		public void StopListener()
		{
			_session.Dispose();
			_session = null;

			_sessionManager.Dispose();
			_sessionManager = null;
		}
	}
}
