using RedsunLibrary.Network;
using RedsunLibrary.Utils;
using RedsunServer.ServerUtils;

namespace RedsunServer.Protocols
{
	public class Req_Login : MessageProcessor<CSLoginReq, SCLoginAck>
	{
		public Req_Login(PacketMessage packet) : base(packet, (Int32)EPacketProtocol.CsLoginReq, (Int32)EPacketProtocol.ScLoginAck)
		{ }

		public override ServerResult onProcess()
		{
			var result = new ServerResult();

			if (ReqMsg.UserName.IsNullOrEmpty())
			{
				return result.setFail(10, "no name");
			}

			return result.setOk();
		}

		public override void onCleanup(ServerResult result)
		{
			Send();
		}
	}
}
