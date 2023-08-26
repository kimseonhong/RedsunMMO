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
        public EObjectType ObjectType { get; init; }

        public AGameObject(EObjectType type)
        {
            ObjectType = type;
        }


        //public void SetPosition(float x, float y);

        ////public bool IsInRange

        //public void Update(double elapsedSec);
    }
}
