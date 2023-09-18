using Google.Protobuf;
using RedsunLibrary.Network;

namespace RedsunClient.Core.Network
{
	public class GameServerNetwork
	{
		// Singleton
		public static GameServerNetwork Instance { get; private set; } = new GameServerNetwork();


		private SimpleClientSocket mSocket = new SimpleClientSocket();



		public bool SendCSLoginReq(string username)
		{
			if (string.IsNullOrWhiteSpace(username))
				return false;

			// need to Contents Common Const
			if (username.Length > 100)
				return false;

			if (!mSocket.IsConnected)
				return false;

			CSLoginReq req = new CSLoginReq();
			req.UserName = username;

			bool bRet = _SendPacket(EPacketProtocol.CsLoginReq, req);
			return bRet;
		}

		public bool SendCSJoinGameReq(float x, float y)
		{
			if (!mSocket.IsConnected)
				return false;

			CSJoinGameReq req = new CSJoinGameReq();
			req.EnterPosition ??= new MPosition();
			req.EnterPosition.X = x;
			req.EnterPosition.Y = y;

			bool bRet = _SendPacket(EPacketProtocol.CsJoinGameReq, req);
			return bRet;
		}

		public bool SendCSMoveReq(float x, float y)
		{
			if (!mSocket.IsConnected)
				return false;

			CSMoveReq req = new CSMoveReq();
			req.MoveTargetPosition ??= new MPosition();
			req.MoveTargetPosition.X = x;
			req.MoveTargetPosition.Y = y;
			bool bRet = _SendPacket(EPacketProtocol.CsMoveReq, req);

			return bRet;
		}

		public bool SendCSMoveEndReq(float x, float y)
		{
			if (!mSocket.IsConnected)
				return false;

			CSMoveEndReq req = new CSMoveEndReq();
			req.MoveEndPosition ??= new MPosition();
			req.MoveEndPosition.X = x;
			req.MoveEndPosition.Y = y;
			bool bRet = _SendPacket(EPacketProtocol.CsMoveEndReq, req);
			return bRet;
		}

		public bool SendCSUserInfoReq(List<int> seqs)
		{
			if (seqs.Count == 0)
				return false;

			if (!mSocket.IsConnected)
				return false;

			CSUserInfoReq req = new CSUserInfoReq();
			req.UserSeqList.AddRange(seqs);

			bool bRet = _SendPacket(EPacketProtocol.CsUserInfoReq, req);
			return bRet;
		}

		public bool SendCSMonsterInfoReq(List<int> seqs)
		{
			if (seqs.Count == 0)
				return false;

			if (!mSocket.IsConnected)
				return false;

			CSMonsterInfoReq req = new CSMonsterInfoReq();
			req.MonsterSeqList.AddRange(seqs);

			bool bRet = _SendPacket(EPacketProtocol.CsMonsterInfoReq, req);
			return bRet;

		}





		private bool _SendPacket<T>(EPacketProtocol protocol, T msg) where T : Google.Protobuf.IBufferMessage
		{
			if (!mSocket.IsConnected)
				return false;

			int nSize = msg.CalculateSize();
			if (nSize < 0
				|| nSize > PacketConst.MAX_PACKET_BODY_SIZE)
				return false;


			// 최적화는 나중에
			var buffer = msg.ToByteArray();

			Packet packet = new Packet((int)protocol, buffer, 0, buffer.Length);
			//packet.SetBody(buffer, 0, buffer.Length);
			// PacketToByteArray() 함수 호출 시, CheckSum 생성
			//packet._MakeBodyCheckSum(); // private 함수인거같은데

			mSocket.Send(packet);
			return true;
		}
	}
}
