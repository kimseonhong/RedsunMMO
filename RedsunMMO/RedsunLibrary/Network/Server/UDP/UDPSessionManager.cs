using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RedsunLibrary.Network.UDP
{
	public class UDPSessionManager : IDisposable
	{
		class SessionIdAllocate
		{
			// 1조
			private Int64 _defaultId = 1000000000000;
			private Int64 _currentAllocId = 0;

			public void Init(Int64 defaultId = 1000000000000)
			{
				_defaultId = defaultId;
				_currentAllocId = 0;
			}

			public Int64 AllocSessionId()
			{
				_currentAllocId += 1;

				return _defaultId + _currentAllocId;
			}
		}

		private object _lockObj;
		private bool _flexible = false; // true 라면 유연하게(세션을 다 사용중이더라도 새로 생성할 수 있음), false 라면 강력하게(세션 다 사용하면 끝)
		private SessionIdAllocate _sessionIdAllocate;

		private Dictionary<EndPoint, UDPSession> _sessionIdToEndPoint;
		private Dictionary<Int64 /* SessionId */, UDPSession> _sessionList;
		private Queue<UDPSession> _sessionQueue;

		private UDPSession _listenerSession;
		private IUDPSessionEventHandler _sessionEventHandler;

		public UDPSessionManager(IUDPSessionEventHandler sessionEventHandler, bool flexible = false)
		{
			_lockObj = new object();
			_flexible = flexible;
			_sessionIdAllocate = new SessionIdAllocate();

			_sessionIdToEndPoint = new Dictionary<EndPoint, UDPSession>();
			_sessionList = new Dictionary<Int64, UDPSession>();
			_sessionQueue = new Queue<UDPSession>();

			_sessionEventHandler = sessionEventHandler;
		}

		public void Initalize(UDPSession listenerSession, int poolSize)
		{
			_listenerSession = listenerSession;
			for (int i = 0; i < poolSize; i++)
			{
				UDPSession session = new UDPSession(_listenerSession, this, _sessionEventHandler);
				_sessionQueue.Enqueue(session);
			}
		}

		public UDPSession FindOrPopSession(EndPoint endpoint)
		{
			if (_sessionIdToEndPoint.TryGetValue(endpoint, out var session))
			{
				return session;
			}
			return PopSession(endpoint);
		}

		public UDPSession PopSession(EndPoint endpoint)
		{
			UDPSession session;

			lock (_lockObj)
			{
				// 유연하지 않고 다 사용중이라면 null return
				if (_sessionQueue.Count == 0)
				{
					if (_flexible == false)
					{
						_sessionEventHandler.onConnectFailed(0, "Full Connection");
						return null;
					}
					session = new UDPSession(_listenerSession, this, _sessionEventHandler);
				}
				else
				{
					session = _sessionQueue.Dequeue();
				}

				Int64 sessionId = _sessionIdAllocate.AllocSessionId();
				session.AcceptSession(sessionId, endpoint);
				_sessionList.Add(sessionId, session);
				_sessionIdToEndPoint.Add(endpoint, session);
			}
			return session;
		}

		public void PushSession(UDPSession session)
		{
			lock (_lockObj)
			{
				_sessionList.Remove(session.GetSessionId());
				_sessionIdToEndPoint.Remove(session.EndPoint);
				_sessionQueue.Enqueue(session);
			}
			return;
		}

		public Dictionary<Int64, UDPSession> GetConnectUDPSessions()
		{
			return _sessionList;
		}
		public List<UDPSession> GetConnectedUDPSessionsByList()
		{
			return _sessionList.Values.ToList();
		}

		public void Dispose()
		{
			_sessionIdToEndPoint.Clear();
			_sessionIdToEndPoint = null;

			foreach (var data in _sessionList)
			{
				data.Value.Dispose();
			}
			_sessionList.Clear();
			_sessionList = null;

			foreach (var data in _sessionQueue)
			{
				data.Dispose();
			}
			_sessionQueue.Clear();
			_sessionQueue = null;
		}
	}
}
