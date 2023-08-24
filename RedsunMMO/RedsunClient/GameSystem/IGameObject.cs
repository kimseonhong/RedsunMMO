using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedsunClient.GameSystem
{
    // Todo : 인터페이스 or abstract 로 구현
    // Todo : Manager 만들것.

    public interface IGameObject
    {
        public EObjectType GetObjectType();

        public void SetPosition(float x, float y);

        //public bool IsInRange

        public void Update(double elapsedSec);
    }
}
