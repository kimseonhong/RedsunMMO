using System;

namespace RedsunLibrary.Network.UDP
{
	//public interface IUDPSessionEventHandler
	//{
	//	bool onConnected(UDPSession sesson);
	//	bool onConnectFailed(Int64 sessionId, string reason);
	//	void onReceived(UDPSession sesson, Packet packet);
	//	void onInvaliedReceived(UDPSession sesson, Exception ex = null);
	//	void onInvaliedSent(UDPSession sesson, Exception ex = null);
	//}

	public interface IUDPSessionEventHandler
	{
		bool onConnected(UDPSession sesson);
		bool onConnectFailed(Int64 sessionId, string reason);
		void onReceived(UDPSession sesson, Packet packet);
		void onInvaliedReceived(UDPSession sesson, Exception ex = null);
		void onInvaliedSent(UDPSession sesson, Exception ex = null);
	}
}
