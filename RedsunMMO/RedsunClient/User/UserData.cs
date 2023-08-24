using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace RedsunClient.UserData
{
    public class UserData
    {
        // Singleton Object
        public static UserData Instance = new UserData();



        public int MySequence { get; private set; } = 0;
        public string MyName { get; private set; } = string.Empty;

        public float PositionX { get; private set; } = 0.0f;
        public float PositionY { get; private set; } = 0.0f;


        public float BeforePositionX { get; private set; } = 0.0f;
        public float BeforePositionY { get; private set; } = 0.0f;


        public void Reset()
        {
            MySequence = 0;
            MyName = string.Empty;

            PositionX = PositionY = BeforePositionX = BeforePositionY = 0.0f;
        }




        // from Packet

        public void OnSCLoginAck(SCLoginAck ack)
        {
            MySequence = ack.Seq;
            MyName = ack.UserName;
        }


        public void OnSCJoinGameAck(SCJoinGameAck ack)
        {
            if (null == ack.EnterPosition)
                return;

            PositionX = ack.EnterPosition.X;
            PositionY = ack.EnterPosition.Y;
        }

        public void OnSCMoveAck(SCMoveAck ack)
        {
            if (ack.Seq != MySequence)
                return;

            if (null == ack.MoveTargetPosition)
                return;

            BeforePositionX = ack.MoveTargetPosition.X;
            BeforePositionY = ack.MoveTargetPosition.Y;
        }

        public void OnSCMoveEndAck(SCMoveEndAck ack)
        {
            if(ack.Seq != MySequence) 
                return;

            if (null == ack.MoveEndPosition)
                return;

            BeforePositionX = ack.MoveEndPosition.X;
            BeforePositionY = ack.MoveEndPosition.Y;
        }

    }
}
