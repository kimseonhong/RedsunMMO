using Google.Protobuf;
using RedsunLibrary.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedsunClient.Core.Network
{
    public class GameServerNetwork
    {
        // Singleton
        public static GameServerNetwork Instance { get; private set; } = new GameServerNetwork();


        private SimpleClientSocket mSocket = new SimpleClientSocket();



        public bool SendCSLoginReq(string username)
        {
            if (!mSocket.IsConnected)
                return false;

            if (string.IsNullOrWhiteSpace(username))
                return false;

            // need to Contents Common Const
            if (username.Length > 100)
                return false;

            CSLoginReq req = new CSLoginReq();
            req.UserName = username;

            bool bRet = _SendPacket(EPacketProtocol.CsLoginReq, req);
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

            Packet packet = new Packet();
            //packet.pro
            // how to set Protocol???

            packet.SetBody(buffer, 0, buffer.Length);
            packet._MakeBodyCheckSum(); // private 함수인거같은데

            mSocket.Send(packet);
            return true;
        }
    }
}
