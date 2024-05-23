using System;
using System.Collections.Generic;
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

			_sessionManager = new UDPSessionManager(_sessionEventHandler);
			_session = new UDPSession(_endPoint, _sessionManager, _sessionEventHandler);

			_sessionManager.Initalize(_session, DEFAULT_POOL_SIZE);
			_session.Bind();

			// 시작
			_session.ReceiveAsync();
		}

		public void SendToClientAll(Packet packet)
		{
			foreach (var session in _sessionManager.GetConnectUDPSessions())
			{
				session.Value.Send(packet);
				//_session.SendAsync(packet, session.Value.EndPoint);
			}
		}

		public void SendToServer(Packet packet)
		{
			_session.Send(packet);
		}

		public void StopListener()
		{
			_session?.Dispose();
			_session = null;

			_sessionManager?.Dispose();
			_sessionManager = null;
		}
	}
}
