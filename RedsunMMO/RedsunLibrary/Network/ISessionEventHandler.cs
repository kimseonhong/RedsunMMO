using RedsunLibrary.Network.Server;
using System;

namespace RedsunLibrary.Network
{
	public interface ISessionEventHandler
	{
		bool onConnected(Session sesson);
		bool onConnectFailed(string reason);
		void onDisconnected(Session sesson);
		void onReceived(Session sesson, Packet packet);
		void onInvaliedReceived(Session sesson, Exception ex = null);
		void onInvaliedSent(Session sesson, Exception ex = null);
	}
}
