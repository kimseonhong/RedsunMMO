using System;

namespace RedsunLibrary.Network.Server
{
	public interface ISession : IDisposable
	{
		Int64 GetSessionId();

		void ReceiveAsync(int pendingCount = 0);
		void Send(Packet packet);
		void Close();
	}
}
