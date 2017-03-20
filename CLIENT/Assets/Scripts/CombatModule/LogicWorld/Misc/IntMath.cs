using System;

class IntMath
{
    public const int FLOAT_MAGNIFICATION = 10000;
    public const int METER_MAGNIFICATION = 100;
    public const int INT_PI = 31416;
    public const int INT_TWOPI = 61832;
    public const int INT_HALFPI = 15708;
    public const int INT_QUARTERPI = 7854;

    public static int Radian2DegreeI(int int_rad)
    {
        int deg = int_rad * 180 / INT_PI;
        if (deg < 0)
            deg += 360;
        deg %= 360;
        return deg;
    }

    public static int Degree2RadianI(int deg)
    {
        return deg * INT_PI / 180;
    }

    public static int SinI(int degree)
    {
        degree %= 360;
        if (degree < 0)
            degree += 360;
        return SinCosTable.sintable[degree];
    }

    public static int CosI(int degree)
    {
        degree %= 360;
        if (degree < 0)
            degree += 360;
        return SinCosTable.costable[degree];
    }

    public static int XZToDegree(int x, int z)
    {
        //ZZWTODO 可以自己实现Atan2I
        int int_rad = (int)(Math.Atan2(z, x) * FLOAT_MAGNIFICATION);
        return Radian2DegreeI(int_rad);
    }

    public static int Distance2D(int x, int z)
    {
        if (x < 0)
            x = -x;
        if (z < 0)
            z = -z;
        int min_xz = x;
        if (z < x)
            min_xz = z;
        return x + z - (min_xz >> 1) - (min_xz >> 2) + (min_xz >> 4);
    }
}