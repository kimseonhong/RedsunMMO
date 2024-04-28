using RedsunLibrary;
using RedsunLibrary.Network;
using RedsunLibrary.Network.Server;
using RedsunLibrary.Network.TCP;
using RedsunLibrary.Utils;
using RedsunServer.Protocols;
using System.Net;

namespace RedsunServer
{
	public class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello, World!");

			Server server = new Server();
			server.Start();
		}
	}

	public class Server : ServerLogic, ITCPSessionEventHandler
	{
		private TCPServerListener _listener;

		public Server()
		{
			_listener = new TCPServerListener(this);
		}

		public new void Start()
		{
			_listener.StartListener(IPAddress.Any, 15648);
			base.Start();

			Console.WriteLine("Press Enter, Stop to Server");
			Console.ReadLine();

			_listener.StopListener();
			base.Stop();
		}

		public override void GameLogicUpdate()
		{

		}

		public override void PacketProcessor(PacketMessage message)
		{
			switch (message.GetPacketId())
			{
				case (int)EPacketProtocol.CsLoginReq:
					var protocol = new Req_Login(message);
					protocol.HandleProcess();
					break;
			}
		}

		#region ISessionEventHandler
		public bool onConnected(TCPSession sesson)
		{
			Console.WriteLine("Connected!!");
			return true;
		}

		public bool onConnectFailed(string reason)
		{
			Console.WriteLine($"Connect Fail : {reason}");
			return true;
		}

		public void onDisconnected(TCPSession sesson)
		{
			Console.WriteLine("Disconnected");
		}

		public void onInvaliedReceived(TCPSession sesson, Exception ex = null)
		{
			Console.WriteLine($"Session ({sesson.GetSessionId()}) InvaliedReceivedMessage:\n{ex.Message} ");
		}

		public void onInvaliedSent(TCPSession sesson, Exception ex = null)
		{
			Console.WriteLine($"Session ({sesson.GetSessionId()}) InvaliedSentMessage:\n{ex.Message} ");
		}

		public void onReceived(TCPSession sesson, Packet packet)
		{
			AddPacket(sesson, packet);
			Console.WriteLine($"Session ({sesson.GetSessionId()}) Received: {packet.GetPacketId()}");
		}
		#endregion
	}
}
