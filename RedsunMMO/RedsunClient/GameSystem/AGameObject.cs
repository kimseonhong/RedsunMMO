using RedsunClient.Core.Mathmatics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedsunClient.GameSystem
{
    // Todo : 인터페이스 or abstract 로 구현
    // Todo : Manager 만들것.

    public abstract class AGameObject
    {
        public int Seq { get; set; } = 0;
        public EObjectType ObjectType { get; init; }
        public Position CurrentPosition { get; set; } = new Position();
        public Position BeforePosition { get; set; } = new Position();
        public Position MoveTargetPosition { get; set; } = new Position();

        public bool IsAbailable => mInitialized;

        protected bool mInitialized = false;


        public AGameObject(EObjectType type)
        {
            ObjectType = type;
        }


        public abstract void Update(float elapsedSec);
        public abstract void Reset();


        public void OnMove(MPosition pos)
        {
            if (!IsAbailable)
                return;

            MoveTargetPosition.Set(pos.X, pos.Y);
        }


    }
}
