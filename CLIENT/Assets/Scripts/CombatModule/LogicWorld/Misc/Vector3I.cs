using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    public struct Vector3I
    {
        public int x;
        public int y;
        public int z;

        public Vector3I(int a = 0, int b = 0, int c = 0)
        {
            x = a;
            y = b;
            z = c;
        }
        public Vector3I(float a, float b, float c)
        {
            x = (int)a;
            y = (int)b;
            z = (int)c;
        }
        public Vector3I(Vector3I rhs)
        {
            x = rhs.x;
            y = rhs.y;
            z = rhs.z;
        }
        public void Set(int a, int b, int c)
        {
            x = a;
            y = b;
            z = c;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }
        public override bool Equals(object rhs)
        {
            return (rhs is Vector3I) && Equals((Vector3I)rhs);
        }
        public bool Equals(Vector3I rhs)
        {
            return x == rhs.x && y == rhs.y && z == rhs.z;
        }
        public override int GetHashCode()
        {
            return 0;
        }

        public static Vector3I operator -(Vector3I v3i)
        {
            return new Vector3I(-v3i.x, -v3i.y, -v3i.z);
        }
        public static Vector3I operator +(Vector3I lhs, Vector3I rhs)
        {
            return new Vector3I(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }
        public static Vector3I operator -(Vector3I lhs, Vector3I rhs)
        {
            return new Vector3I(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }
        public static Vector3I operator *(int lhs, Vector3I rhs)
        {
            return new Vector3I(rhs.x * lhs, rhs.y * lhs, rhs.z * lhs);
        }
        public static Vector3I operator *(Vector3I lhs, int rhs)
        {
            return new Vector3I(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs);
        }
        public static Vector3I operator /(Vector3I lhs, int rhs)
        {
            if (rhs == 0)
                return new Vector3I(int.MaxValue, int.MaxValue, int.MaxValue);
            else
                return new Vector3I(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
        }

        public static bool operator ==(Vector3I lhs, Vector3I rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }
        public static bool operator !=(Vector3I lhs, Vector3I rhs)
        {
            return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
        }

        public int Dot(Vector3I v3i)
        {
            return x * v3i.x + y * v3i.y + z * v3i.z;
        }

        public Vector3I Cross(Vector3I v3i)
        {
            return new Vector3I(
                y * v3i.z - z * v3i.y,
                z * v3i.x - x * v3i.z,
                x * v3i.y - y * v3i.x
            );
        }

        public void Zero()
        {
            x = y = z = 0;
        }
        public void UnitX()
        {
            x = 1;
            y = z = 0;
        }
        public void UnitY()
        {
            y = 1;
            x = z = 0;
        }
        public void UnitZ()
        {
            z = 1;
            x = y = 0;
        }
        public void One()
        {
            x = y = z = 1;
        }

        public int LengthSquare()
        {
            return x * x + y * y + z * z;
        }
        
        public int Length()
        {
            int temp = IntMath.Distance2D(x, z);
            return IntMath.Distance2D(temp, y);
        }

        public int DistanceSquare(ref Vector3I v3i)
        {
            int dx = v3i.x - x;
            int dy = v3i.y - y;
            int dz = v3i.z - z;
            return dx * dx + dy * dy + dz * dz;
        }

        public int Distance(ref Vector3I v2i)
        {
            int temp = IntMath.Distance2D(v2i.x - x, v2i.z - z);
            return IntMath.Distance2D(temp, v2i.y - y);
        }

        public int Normalize()
        {
            int length = Length();
            if (length == 0)
                return 0;
            x = x * IntMath.METER_MAGNIFICATION / length;
            y = y * IntMath.METER_MAGNIFICATION / length;
            z = z * IntMath.METER_MAGNIFICATION / length;
            return length;
        }

        public static Vector3I Lerp(Vector3I from, Vector3I to, int cur_t, int total_t, Vector3I result)
        {
            if (cur_t > total_t)
                cur_t = total_t;
            result.x = from.x + (to.x - from.x) * cur_t / total_t;
            result.y = from.y + (to.y - from.y) * cur_t / total_t;
            result.z = from.z + (to.z - from.z) * cur_t / total_t;
            return result;
        }
    };
}