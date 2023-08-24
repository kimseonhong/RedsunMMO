using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedsunClient.GameSystem
{
    public enum EObjectType
    {
        None = 0,

        Player = 1,
        Monster = 2,
        
        Max,
    }

    public static class EObjectTypeExtension
    {
        public static bool IsValid(this EObjectType type)
        {
            switch(type)
            {
                case EObjectType.Player:
                case EObjectType.Monster:
                    return true;
                default:
                    break;
            }
            return false;
        }
    }

}
