using System;

namespace RedsunLibrary.Network.TCP
{
	public interface ITCPSessionEventHandler
	{
		bool onConnected(TCPSession sesson);
		bool onConnectFailed(Int64 sessionId, string reason);
		void onDisconnected(Int64 sessionId);
		void onReceived(TCPSession sesson, Packet packet);
		void onInvaliedReceived(TCPSession sesson, Exception ex = null);
		void onInvaliedSent(TCPSession sesson, Exception ex = null);
	}
}
