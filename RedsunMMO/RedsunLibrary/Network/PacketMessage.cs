using RedsunLibrary.Network.Server;
using System;

namespace RedsunLibrary.Network
{
	public class PacketMessage
	{
		protected ISession _session = null;
		protected Packet _packet = null;

		public ISession Session => _session;
		public Packet Packet => _packet;

		public Int64 SessionId => _session.GetSessionId();

		public PacketMessage(ISession session, Packet packet)
		{
			_session = session;
			_packet = packet;
		}

		public Int32 GetPacketId()
		{
			return _packet.GetPacketId();
		}
	}
}
