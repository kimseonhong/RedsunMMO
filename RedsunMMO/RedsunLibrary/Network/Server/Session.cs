using System;
using System.Net.Sockets;

namespace RedsunLibrary.Network.Server
{
	public class Session : RawSocket, IDisposable
	{
		private SocketAsyncEventArgs _receiveEventArgs;
		private SocketAsyncEventArgs _sendEventArgs;

		public Session() : base()
		{

		}
	}
}
