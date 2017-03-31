using System;
using System.Collections;
using System.Collections.Generic;
namespace Combat
{
    /*
     * 单位统一为厘米
     * Normalize之后其长度为100（IntMath.METER_MAGNIFICATION）
     * XZ对应Unity的XZ：X轴朝右，Y轴朝上，Z轴朝前，顺时针逆时针都是从上往下看的，即-Y轴视角
     * 也可以按需定义为XY平面，这时候用XY属性，这时候假设Z轴朝向屏幕外
    */
    public struct Vector2I : IEquatable<Vector2I>
    {
        public int x;
        public int z;

        public Vector2I(int a = 0, int b = 0)
        {
            x = a;
            z = b;
        }
        public Vector2I(float a, float b)
        {
            x = (int)a;
            z = (int)b;
        }
        public Vector2I(ref Vector2I rhs)
        {
            x = rhs.x;
            z = rhs.z;
        }

        public override int GetHashCode()
        {
            return 0;
        }
        public override bool Equals(object rhs)
        {
            return (rhs is Vector2I) && Equals((Vector2I)rhs);
        }
        public bool Equals(Vector2I rhs)
        {
            return x == rhs.x && z == rhs.z;
        }
        public override string ToString()
        {
            return "(" + x + ", " + z + ")";
        }

        public static Vector2I operator -(Vector2I v2i)
        {
            return new Vector2I(-v2i.x, -v2i.z);
        }

        public static Vector2I operator +(Vector2I lhs, Vector2I rhs)
        {
            return new Vector2I(lhs.x + rhs.x, lhs.z + rhs.z);
        }
        public static Vector2I operator -(Vector2I lhs, Vector2I rhs)
        {
            return new Vector2I(lhs.x - rhs.x, lhs.z - rhs.z);
        }
        public static Vector2I operator *(Vector2I lhs, int rhs)
        {
            return new Vector2I(lhs.x * rhs, lhs.z * rhs);
        }
        public static Vector2I operator *(int lhs, Vector2I rhs)
        {
            return new Vector2I(rhs.x * lhs, rhs.z * lhs);
        }
        public static Vector2I operator /(Vector2I lhs, int rhs)
        {
            if (rhs == 0)
                return new Vector2I(int.MaxValue, int.MaxValue);
            else
                return new Vector2I(lhs.x / rhs, lhs.z / rhs);
        }
        public static bool operator ==(Vector2I lhs, Vector2I rhs)
        {
            return lhs.x == rhs.x && lhs.z == rhs.z;
        }
        public static bool operator !=(Vector2I lhs, Vector2I rhs)
        {
            return lhs.x != rhs.x || lhs.z != rhs.z;
        }

        public int Normalize()
        {
            int length = Length();
            if (length == 0)
                return 0;
            x = x * IntMath.METER_MAGNIFICATION / length;
            z = z * IntMath.METER_MAGNIFICATION / length;
            return length;
        }

        public int Dot(ref Vector2I v2i)
        {
            return x * v2i.x + z * v2i.z;
        }

        public int LengthSquare()
        {
            return x * x + z * z;
        }

        public int Length()
        {
            return IntMath.Distance2D(x, z);
        }

        public int DistanceSquare(ref Vector2I v2i)
        {
            int dx = v2i.x - x;
            int dz = v2i.z - z;
            return dx * dx + dz * dz;
        }

        public int Distance(ref Vector2I v2i)
        {
            return IntMath.Distance2D(v2i.x - x, v2i.z - z);
        }

        public int ScaleToLength(int expected_length)
        {
            int length = Length();
            if (length == 0)
                return 0;
            x = x * expected_length / length;
            z = z * expected_length / length;
            return length;
        }

        public int Truncate(int max_len)
        {
            int length = Length();
            if (length > max_len)
            {
                x = x * max_len / length;
                z = z * max_len / length;
            }
            return length;
        }

        public void Scale(int multiply, int divide)
        {
            x = x * multiply / divide;
            z = z * multiply / divide;
        }

        public void MakeZero()
        {
            x = 0;
            z = 0;
        }
        public void Reverse()
        {
            x = -x;
            z = -z;
        }

        //v2i在当前向量的哪个方向
        public enum VectorSign { CLOCKWISE = 1, ANTICLOCKWISE = -1 };
        public VectorSign Sign(ref Vector2I v2i)
        {
            if (z * v2i.x > x * v2i.z)
                return VectorSign.CLOCKWISE;
            else
                return VectorSign.ANTICLOCKWISE;
        }

        public Vector2I Perpendicular()
        {
            return new Vector2I(-z, x);
        }
        public void Perpendicular(ref Vector2I v2i)
        {
            v2i.x = -z;
            v2i.z = x;
        }

        public Vector2I PerpClockwise()
        {
            return new Vector2I(z, -x);
        }
        public void PerpClockwise(ref Vector2I v2i)
        {
            v2i.x = z;
            v2i.z = -x;
        }

        public Vector2I PerpAnticlockwise()
        {
            return new Vector2I(-z, x);
        }
        public void PerpAnticlockwise(ref Vector2I v2i)
        {
            v2i.x = -z;
            v2i.z = x;
        }

        // 与normalized_basis平行的分量
        public Vector2I ParallelComponent(ref Vector2I normalized_basis)
        {
            int projection = this.Dot(ref normalized_basis) / 100;
            return normalized_basis * projection;
        }

        // 与normalized_basis垂直的分量
        public Vector2I PerpendicularComponent(ref Vector2I normalized_basis)
        {
            return this - ParallelComponent(ref normalized_basis);
        }

        // 反射（norm是Normalize()之后的向量，即长度是100）
        // (-2,-1)从右边撞上墙X=0（其norm是(100,0)）后，反射为(2, -1)
        public Vector2I GetReflect(ref Vector2I norm)
        {
            Vector2I reflect = this.Dot(ref norm) * norm;
            reflect /= 5000;
            return this - reflect;
        }

        public int ToDegree()
        {
            //ZZWTODO 和美术资源的默认朝向相关
            return IntMath.XZToDegree(x, z);
        }

        public void FromDegree(int degree)
        {
            //ZZWTODO 和美术资源的默认朝向相关
            x = IntMath.CosI(degree);
            z = IntMath.SinI(degree);
            Normalize();
        }

        public static bool InsideRegion(ref Vector2I p, ref Vector2I min_xz, ref Vector2I max_xz)
        {
            return !(p.x < min_xz.x || p.z < min_xz.z || p.x > max_xz.x || p.z > max_xz.z);
        }

        // 判断target是否在以source为中心的朝向为facing的角度范围fov_deg里面
        public static bool InsideFov(ref Vector2I source, ref Vector2I facing, int fov_deg, ref Vector2I target)
        {
            Vector2I to_target = target - source;
            to_target.Normalize();
            return to_target.Dot(ref facing) >= IntMath.CosI(fov_deg / 2);
        }

        static public void Lerp(ref Vector2I from, ref Vector2I to, int cur_t, int total_t, ref Vector2I result)
        {
            if (cur_t >= total_t)
            {
                result = to;
                return;
            }
            result.x = from.x + (to.x - from.x) * cur_t / total_t;
            result.z = from.z + (to.z - from.z) * cur_t / total_t;
        }

        static public Vector2I Lerp(ref Vector2I from, ref Vector2I to, int cur_t, int total_t)
        {
            if (cur_t >= total_t)
                return to;
            int result_x = from.x + (to.x - from.x) * cur_t / total_t;
            int result_z = from.z + (to.z - from.z) * cur_t / total_t;
            return new Vector2I(result_x, result_z);
        }
    }
}