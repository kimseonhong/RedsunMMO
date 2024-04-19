using RedsunLibrary.Network.Server;
using System;

namespace RedsunLibrary.Network
{
	public class PacketMessage
	{
		protected Session _session = null;
		protected Packet _packet = null;

		public Session Session => _session;
		public Packet Packet => _packet;

		public Int64 SessionId => _session.SessionId;

		public PacketMessage(Session session, Packet packet)
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
