using System;
using System.IO;

public partial struct FixPoint : IEquatable<FixPoint>, IComparable<FixPoint>
{
    readonly long m_raw_value;

    public FixPoint(int value)
    {
        m_raw_value = value * ONE;
    }

    public static FixPoint Parse(string str)
    {
        int index = str.IndexOf('/');
        if (index == 0)
        {
            return new FixPoint(int.Parse(str));
        }
        else
        {
            string temp = str.Substring(0, index);
            FixPoint a = new FixPoint(int.Parse(temp));
            temp = str.Substring(index + 1);
            FixPoint b = new FixPoint(int.Parse(temp));
            return a / b;
        }
    }

    public static readonly decimal Precision = (decimal)(new FixPoint(1L));//0.0000152587890625m
    public static readonly FixPoint MaxValue = new FixPoint(MAX_VALUE);
    public static readonly FixPoint MinValue = new FixPoint(MIN_VALUE);
    public static readonly FixPoint One = new FixPoint(ONE);
    public static readonly FixPoint Zero = new FixPoint(0L);
    public static readonly FixPoint PrecisionFP = new FixPoint(1L);

    public static readonly FixPoint Pi = new FixPoint(PI);
    public static readonly FixPoint TwoPi = new FixPoint(TWO_PI);
    public static readonly FixPoint HalfPi = new FixPoint(HALF_PI);
    public static readonly FixPoint QuarterPi = new FixPoint(QUARTER_PI);
    public static readonly FixPoint InvPi = new FixPoint(INV_PI);

    public static explicit operator FixPoint(long value)
    {
        return new FixPoint(value * ONE);
    }
    #region 正式的时候不开放这几个
    public static explicit operator FixPoint(float value)
    {
        return new FixPoint((long)(value * ONE));
    }
    public static explicit operator FixPoint(double value)
    {
        return new FixPoint((long)(value * ONE));
    }
    public static explicit operator FixPoint(decimal value)
    {
        return new FixPoint((long)(value * ONE));
    }
    #endregion

    public static explicit operator int(FixPoint value)
    {
        return (int)(value.m_raw_value >> FRACTIONAL_PLACES);
    }
    public static explicit operator long(FixPoint value)
    {
        return value.m_raw_value >> FRACTIONAL_PLACES;
    }
    public static explicit operator float(FixPoint value)
    {
        return (float)value.m_raw_value / ONE;
    }
    public static explicit operator double(FixPoint value)
    {
        return (double)value.m_raw_value / ONE;
    }
    public static explicit operator decimal(FixPoint value)
    {
        return (decimal)value.m_raw_value / ONE;
    }

    public override bool Equals(object obj)
    {
        return obj is FixPoint && ((FixPoint)obj).m_raw_value == m_raw_value;
    }
    public override int GetHashCode()
    {
        return m_raw_value.GetHashCode();
    }
    public bool Equals(FixPoint other)
    {
        return m_raw_value == other.m_raw_value;
    }
    public int CompareTo(FixPoint other)
    {
        return m_raw_value.CompareTo(other.m_raw_value);
    }
    public override string ToString()
    {
        return ((decimal)this).ToString();
    }

    public static FixPoint operator +(FixPoint x, FixPoint y)
    {
        return new FixPoint(x.m_raw_value + y.m_raw_value);
    }
    public static FixPoint operator -(FixPoint x, FixPoint y)
    {
        return new FixPoint(x.m_raw_value - y.m_raw_value);
    }
    public static FixPoint operator *(FixPoint x, FixPoint y)
    {
        return new FixPoint((x.m_raw_value * y.m_raw_value) >> FRACTIONAL_PLACES);
    }
    public static FixPoint operator /(FixPoint x, FixPoint y)
    {
        return new FixPoint((x.m_raw_value << FRACTIONAL_PLACES) / y.m_raw_value);
    }
    public static FixPoint operator %(FixPoint x, FixPoint y)
    {
        return new FixPoint(x.m_raw_value % y.m_raw_value);
    }

    public static FixPoint operator -(FixPoint x)
    {
        return x.m_raw_value == MIN_VALUE ? MaxValue : new FixPoint(-x.m_raw_value);
    }
    public static bool operator ==(FixPoint x, FixPoint y)
    {
        return x.m_raw_value == y.m_raw_value;
    }
    public static bool operator !=(FixPoint x, FixPoint y)
    {
        return x.m_raw_value != y.m_raw_value;
    }
    public static bool operator >(FixPoint x, FixPoint y)
    {
        return x.m_raw_value > y.m_raw_value;
    }
    public static bool operator <(FixPoint x, FixPoint y)
    {
        return x.m_raw_value < y.m_raw_value;
    }
    public static bool operator >=(FixPoint x, FixPoint y)
    {
        return x.m_raw_value >= y.m_raw_value;
    }
    public static bool operator <=(FixPoint x, FixPoint y)
    {
        return x.m_raw_value <= y.m_raw_value;
    }

    public static int Sign(FixPoint value)
    {
        return -(int)((ulong)(value.m_raw_value) >> 63) | (int)((ulong)(-value.m_raw_value) >> 63);
    }

    public static FixPoint Abs(FixPoint value)
    {
        long mask = value.m_raw_value >> 63;
        return new FixPoint((value.m_raw_value + mask) ^ mask);
    }

    public static FixPoint Floor(FixPoint value)
    {
        return new FixPoint((long)((ulong)value.m_raw_value & 0xFFFFFFFFFFFF0000));
    }

    public static FixPoint Ceiling(FixPoint value)
    {
        bool has_fractional_part = (value.m_raw_value & 0x000000000000FFFF) != 0;
        return has_fractional_part ? Floor(value) + One : value;
    }

    public static FixPoint Sqrt(FixPoint value)
    {
        long x = value.m_raw_value << FRACTIONAL_PLACES;
        if (x <= 1)
            return value;
        int s = 1;
        long x1 = x - 1;
        if (x1 > 4294967295) { s += 16; x1 >>= 32; }
        if (x1 > 65535) { s += 8; x1 >>= 16; }
        if (x1 > 255) { s += 4; x1 >>= 8; }
        if (x1 > 15) { s += 2; x1 >>= 4; }
        if (x1 > 3L) { s += 1; }
        long g0 = 1L << s;
        long g1 = (g0 + (x >> s)) >> 1;
        while (g1 < g0)
        {
            g0 = g1;
            g1 = (g0 + (x / g0)) >> 1;
        }
        return new FixPoint(g0);
    }

    public static FixPoint FastDistance(FixPoint x, FixPoint y)
    {
        long x1 = x.m_raw_value;
        long y1 = y.m_raw_value;
        if (x1 < 0) x1 = -x1;
        if (y1 < 0) y1 = -y1;
        long min_xy = x1;
        if (y1 < x1) min_xy = y1;
        long result = x1 + y1 - (min_xy >> 1) - (min_xy >> 2) + (min_xy >> 4);
        return new FixPoint(result);
    }

    public static FixPoint Sin(FixPoint radian)
    {
        long raw = radian.m_raw_value % TWO_PI;
        if (raw < 0)
            raw += TWO_PI;
        long p1 = raw % HALF_PI;
        long p2 = raw / HALF_PI;
        if (p2 == 0)
            return new FixPoint(SinTable[p1]);
        else if (p2 == 1)
            return new FixPoint(SinTable[HALF_PI - 1 - p1]);
        else if (p2 == 2)
            return new FixPoint(-SinTable[p1]);
        else
            return new FixPoint(-SinTable[HALF_PI - 1 - p1]);
    }

    public static FixPoint Cos(FixPoint radian)
    {
        long raw = radian.m_raw_value % TWO_PI;
        if (raw < 0)
            raw += TWO_PI;
        long p1 = raw % HALF_PI;
        long p2 = raw / HALF_PI;
        if (p2 == 0)
            return new FixPoint(SinTable[HALF_PI - 1 - p1]);
        else if (p2 == 1)
            return new FixPoint(-SinTable[p1]);
        else if (p2 == 2)
            return new FixPoint(-SinTable[HALF_PI - 1 - p1]);
        else
            return new FixPoint(SinTable[p1]);
    }

    #region 内部
    const int NUM_BITS = 64;
    const int FRACTIONAL_PLACES = 16;
    const long ONE = 1L << FRACTIONAL_PLACES;
    const long MAX_VALUE = long.MaxValue;
    const long MIN_VALUE = long.MinValue;
    //3.14159265358979323846264338327950288419716939937510
    const long PI = 205884L;
    const long TWO_PI = 411768L;
    const long HALF_PI = 102942L;
    const long QUARTER_PI = 51471L;
    const long INV_PI = 20860L;

    internal static void GenerateSinTable()
    {
        using (var writer = new StreamWriter("FixPointSinTable.cs"))
        {
            writer.Write(
@"partial struct FixPoint {
    public static readonly long[] SinTable = new[] {");
            int line_counter = 0;
            for (long i = 0; i < HALF_PI; ++i)
            {
                double angle = i * Math.PI * 0.5 / (HALF_PI - 1);
                if (line_counter++ % 8 == 0)
                {
                    writer.WriteLine();
                    writer.Write("        ");
                }
                double sin = Math.Sin(angle);
                long raw_value = ((FixPoint)sin).m_raw_value;
                writer.Write(string.Format("0x{0:X}L, ", raw_value));
            }
            writer.Write(
@"
    };
}");
        }
    }

    private FixPoint(long raw_value)
    {
        m_raw_value = raw_value;
    }

    public static FixPoint FromRaw(long raw_value)
    {
        return new FixPoint(raw_value);
    }

    public long RawValue
    {
        get { return m_raw_value; }
    }
    #endregion
}