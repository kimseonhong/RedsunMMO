using RedsunLibrary;
using RedsunLibrary.Network;
using RedsunLibrary.Network.Server;
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

	public class Server : ServerLogic, ISessionEventHandler
	{
		private ServerListener _listener;

		public Server()
		{
			_listener = new ServerListener(this);
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
		public bool onConnected(Session sesson)
		{
			Console.WriteLine("Connected!!");
			return true;
		}

		public bool onConnectFailed(string reason)
		{
			Console.WriteLine($"Connect Fail : {reason}");
			return true;
		}

		public void onDisconnected(Session sesson)
		{
			Console.WriteLine("Disconnected");
		}

		public void onInvaliedReceived(Session sesson, Exception ex = null)
		{
			Console.WriteLine($"Session ({sesson.SessionId}) InvaliedReceivedMessage:\n{ex.Message} ");
		}

		public void onInvaliedSent(Session sesson, Exception ex = null)
		{
			Console.WriteLine($"Session ({sesson.SessionId}) InvaliedSentMessage:\n{ex.Message} ");
		}

		public void onReceived(Session sesson, Packet packet)
		{
			AddPacket(sesson, packet);
			Console.WriteLine($"Session ({sesson.SessionId}) Received: {packet.GetPacketId()}");
		}
		#endregion
	}
}
