using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RedsunLibrary.Network.TCP
{
	public class TCPSessionManager : IDisposable
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

		private Dictionary<Int64 /* SessionId */, TCPSession> _sessionList;
		private Queue<TCPSession> _sessionQueue;

		private ITCPSessionEventHandler _sessionEventHandler;

		public TCPSessionManager(ITCPSessionEventHandler sessionEventHandler, bool flexible = false)
		{
			_lockObj = new object();
			_flexible = flexible;
			_sessionIdAllocate = new SessionIdAllocate();

			_sessionList = new Dictionary<Int64, TCPSession>();
			_sessionQueue = new Queue<TCPSession>();
			_sessionEventHandler = sessionEventHandler;
		}

		public void Initalize(int poolSize)
		{
			for (int i = 0; i < poolSize; i++)
			{
				TCPSession session = new TCPSession(this, _sessionEventHandler);
				_sessionQueue.Enqueue(session);
			}
		}

		public TCPSession PopSession(Socket socket)
		{
			TCPSession session;

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
					session = new TCPSession(this, _sessionEventHandler);
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

		public void PushSession(TCPSession session)
		{
			lock (_lockObj)
			{
				_sessionList.Remove(session.GetSessionId());
				_sessionQueue.Enqueue(session);
			}
			return;
		}

		public Dictionary<Int64, TCPSession> GetConnectTCPSessions()
		{
			return _sessionList;
		}

		public void Dispose()
		{
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
