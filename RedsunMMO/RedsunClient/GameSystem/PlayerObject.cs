namespace RedsunClient.GameSystem
{
    public class PlayerObject : AGameObject
    {
        private float DEFAULT_PLAYER_MOVE_SPEED = 5.0f;

        public string Name { get; private set; } = string.Empty;


        public PlayerObject()
            : base(EObjectType.Player)
        {
        }

        public override void Reset()
        {
            Seq = 0;
            CurrentPosition.Reset();
            BeforePosition.Reset();
            MoveTargetPosition.Reset();
            Name = string.Empty;
            mInitialized = false;
        }

        public override void Update(float elapsedSec)
        {
            if (!IsAbailable)
                return;

            if (elapsedSec <= 0.0)
                return;

            _UpdateMove(elapsedSec);

        }



        public bool SetData(MUserInfo info)
        {
            if (IsAbailable)
                return false; // 이미 세팅됨

            if (null == info
                || !info.HasSeq
                || !info.HasUserName)
                return false;

            Seq = info.Seq;
            Name = info.UserName;
            BeforePosition.Set(info.Pos.X, info.Pos.Y);
            CurrentPosition.Set(info.Pos.X, info.Pos.Y);
            MoveTargetPosition.Set(info.Pos.X, info.Pos.Y);
            mInitialized = true;

            return true;
        }



        private void _UpdateMove(float elapsedSec)
        {
            if (MoveTargetPosition.DistanceToPow(CurrentPosition) <= 0.0f)
                return; // 다 이동함

            float fMoveDistance = DEFAULT_PLAYER_MOVE_SPEED * elapsedSec;
            float fDistance = CurrentPosition.DistanceTo(MoveTargetPosition);

            if(fDistance < fMoveDistance)
            {
                BeforePosition.Set(CurrentPosition);
                CurrentPosition = MoveTargetPosition;
            }
            else
            {
                var vDir = MoveTargetPosition - CurrentPosition;
                if (!vDir.Normalize())
                    return; // normalize failure

                BeforePosition.Set(CurrentPosition);
                CurrentPosition.X += vDir.X * fMoveDistance;
                CurrentPosition.Y += vDir.Y * fMoveDistance;
            }
        }


    }
}
