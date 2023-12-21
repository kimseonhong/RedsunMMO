using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RedsunLibrary.Network.Server
{
	public class SessionManager
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

		private Dictionary<Int64 /* SessionId */, Session> _sessionList;
		private Queue<Session> _sessionQueue;

		private ISessionEventHandler _sessionEventHandler;

		public SessionManager(ISessionEventHandler sessionEventHandler, bool flexible = false)
		{
			_lockObj = new object();
			_flexible = flexible;
			_sessionIdAllocate = new SessionIdAllocate();

			_sessionList = new Dictionary<Int64, Session>();
			_sessionQueue = new Queue<Session>();
			_sessionEventHandler = sessionEventHandler;
		}

		public void Initalize(int poolSize)
		{
			for (int i = 0; i < poolSize; i++)
			{
				Session session = new Session(this, _sessionEventHandler);
				_sessionQueue.Enqueue(session);
			}
		}

		public Session PopSession(Socket socket)
		{
			Session session;

			lock (_lockObj)
			{
				// 유연하지 않고 다 사용중이라면 null return
				if (_sessionQueue.Count == 0)
				{
					if (_flexible == false)
					{
						_sessionEventHandler.onConnectFailed("Full Connection");
						return null;
					}
					session = new Session(this, _sessionEventHandler);
				}
				else
				{
					session = _sessionQueue.Dequeue();
				}

				Int64 sessionId = _sessionIdAllocate.AllocSessionId();
				session.AcceptAsyncProcess(sessionId, socket);
				_sessionList.Add(sessionId, session);
			}
			return session;
		}

		public void PushSession(Session session)
		{
			lock (_lockObj)
			{
				_sessionList.Remove(session.SessionId);
				_sessionQueue.Enqueue(session);
			}
			return;
		}
	}
}
