using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace RedsunClient.Core.Mathmatics
{
    public class Position
    {
        public float X
        {
            get
            {
                return mPosition.X;
            }
            set
            {
                mPosition.X = value;
            }
        }

        public float Y
        {
            get
            {
                return mPosition.Y;
            }
            set
            {
                mPosition.Y = value;
            }
        }


        // overloading
        public static Position operator +(Position other) => other;
        public static Position operator +(Position ori, Position other) => new Position(ori.X + other.X, ori.Y + other.Y);
        public static Position operator -(Position other) => new Position(-other.X, -other.Y);
        public static Position operator -(Position ori, Position other) => new Position(ori.X - other.X, ori.Y - other.Y);

        public static Position operator *(Position ori, float v) => new Position(ori.X * v, ori.Y * v);
        public static Position operator /(Position ori, float v) => new Position(ori.X / v, ori.Y / v);

        // end, overliading


        public float LengthPow => mPosition.X * mPosition.X + mPosition.Y * mPosition.Y;
        public float Length => (float)Math.Sqrt(LengthPow);

        private Vector2 mPosition = new Vector2();


        public Position(float x = 0.0f, float y = 0.0f)
        {
            Set(x, y);
        }

        public Position(Position other)
        {
            Set(other.X, other.Y);
        }

        public void Reset() => Set(0.0f, 0.0f);


        public void Set(float x, float y)
        {
            mPosition.X = x;
            mPosition.Y = y;
        }

        public void Set(Position other)
        {
            Set(other.X, other.Y);
        }

        public float DistanceToPow(Position other)
        {
            var gap = other.mPosition - mPosition;
            return gap.X * gap.X + gap.Y * gap.Y;
        }

        public float DistanceTo(Position other) => (float)Math.Sqrt(DistanceToPow(other));


        public bool Normalize()
        {
            float mag = Length;
            if (mag == 0.0f)
                return false;

            X /= mag;
            Y /= mag;

            return true;
        }

        public float GetDirection() => GetDirectionRadian();

        public float GetDirectionRadian()
        {
            Position temp = new Position(this);
            if (!temp.Normalize())
                return float.MinValue; // invalid 

            // clockwise, (0, 1) = 0도

            var fRet = (float)Math.Atan2(temp.X, temp.Y);

            if (fRet < 0)
                fRet += (float)Math.PI * 2.0f;

            return fRet;
        }

        public float GetDirectionDegree()
        {
            return GetDirectionRadian() / (float)Math.PI * 180.0f;
        }


    }
}
