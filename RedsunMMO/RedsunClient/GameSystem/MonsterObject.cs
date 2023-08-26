namespace RedsunClient.GameSystem
{
    public class MonsterObject : AGameObject
    {
        private float DEFAULT_MONSTER_MOVE_SPEED = 2.0f;

        public MonsterObject()
            : base(EObjectType.Monster)
        {
        }

        public override void Reset()
        {
            Seq = 0;
            CurrentPosition.Reset();
            CurrentPosition.Reset();
            BeforePosition.Reset();
            MoveTargetPosition.Reset();
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

        public bool SetData(MMonsterInfo info)
        {
            if (IsAbailable)
                return false;

            Seq = info.Seq;

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

            float fMoveDistance = DEFAULT_MONSTER_MOVE_SPEED * elapsedSec;
            float fDistance = CurrentPosition.DistanceTo(MoveTargetPosition);

            if (fDistance < fMoveDistance)
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
