using System;
using System.IO;

public partial struct FixPoint : IEquatable<FixPoint>, IComparable<FixPoint>
{
    readonly long m_raw_value;

    public FixPoint(int value = 0)
    {
        m_raw_value = value * ONE;
    }

    public static FixPoint CreateFromFloat(float value)
    {
        return new FixPoint((long)(value * ONE));
    }

    public static FixPoint FromRaw(long raw_value)
    {
        return new FixPoint(raw_value);
    }

    public long RawValue
    {
        get { return m_raw_value; }
    }

    public static FixPoint Parse(string str)
    {
        int sign = 0;
        bool fractional = false;
        FixPoint fractional_base = FixPoint.One;
        FixPoint result = FixPoint.Zero;
        for (int i = 0; i < str.Length; ++i)
        {
            char ch = str[i];
            char code = ParseCodeTable[ch];
            if (code == Digit_____)
            {
                int num = ch - '0';
                if (fractional)
                {
                    fractional_base *= FixPoint.Ten;
                    result += FixPointDigit[num] / fractional_base;
                }
                else
                {
                    result *= FixPoint.Ten;
                    result += FixPointDigit[num];
                }
            }
            else if (code == Point_____)
            {
                if (fractional)
                    break;
                fractional = true;
            }
            else if (code == Sign______)
            {
                if (sign != 0 || result.RawValue != 0 || fractional)
                    break;
                if (ch == '-')
                    sign = -1;
                else
                    sign = 1;
            }
            else if (code == WhiteSpace)
                continue;
            else
                break;
        }
        if (sign < 0)
            result = -result;
        return result;
    }

    public static readonly decimal Precision = (decimal)(new FixPoint(1L));//0.0000152587890625m
    public static readonly FixPoint MaxValue = new FixPoint(MAX_VALUE);
    public static readonly FixPoint MinValue = new FixPoint(MIN_VALUE);
    public static readonly FixPoint Zero = new FixPoint(0L);
    public static readonly FixPoint One = new FixPoint(ONE);
    public static readonly FixPoint MinusOne = new FixPoint(-ONE);
    public static readonly FixPoint Two = new FixPoint(ONE * 2L);
    public static readonly FixPoint Ten = new FixPoint(ONE * 10L);
    public static readonly FixPoint Hundred = new FixPoint(ONE * 100L);
    public static readonly FixPoint Thousand = new FixPoint(ONE * 1000L);
    public static readonly FixPoint Million = new FixPoint(ONE * 1000000L);
    public static readonly FixPoint PrecisionFP = new FixPoint(1L);
    public static readonly FixPoint[] FixPointDigit = new[]{
        (FixPoint)0, (FixPoint)1, (FixPoint)2, (FixPoint)3, (FixPoint)4, (FixPoint)5, (FixPoint)6, (FixPoint)7, (FixPoint)8, (FixPoint)9
    };

    public static readonly FixPoint QuarterPi = new FixPoint(QUARTER_PI);
    public static readonly FixPoint HalfPi = new FixPoint(HALF_PI);
    public static readonly FixPoint Pi = new FixPoint(PI);
    public static readonly FixPoint OneAndHalfPi = new FixPoint(ONE_AND_HALF_PI);
    public static readonly FixPoint TwoPi = new FixPoint(TWO_PI);
    public static readonly FixPoint InvPi = new FixPoint(INV_PI);

    public static readonly FixPoint RadianPerDegree = new FixPoint(1144L);
    public static readonly FixPoint DegreePerRadian = new FixPoint(3754936L);

    public static explicit operator FixPoint(int value)
    {
        return new FixPoint(value * ONE);
    }
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

    public override int GetHashCode()
    {
        return m_raw_value.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        return obj is FixPoint && ((FixPoint)obj).m_raw_value == m_raw_value;
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

    public static FixPoint operator -(FixPoint x)
    {
        return x.m_raw_value == MIN_VALUE ? MaxValue : new FixPoint(-x.m_raw_value);
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
        //误差 < 0.002%
        return new FixPoint((x.m_raw_value * y.m_raw_value) >> FRACTIONAL_PLACES);
    }
    public static FixPoint operator /(FixPoint x, FixPoint y)
    {
        //误差 < 0.0000001%
        return new FixPoint((x.m_raw_value << FRACTIONAL_PLACES) / y.m_raw_value);
    }
    public static FixPoint operator %(FixPoint x, FixPoint y)
    {
        return new FixPoint(x.m_raw_value % y.m_raw_value);
    }

    public static FixPoint operator <<(FixPoint fp, int bits)
    {
        return new FixPoint(fp.m_raw_value << bits);
    }
    public static FixPoint operator >>(FixPoint fp, int bits)
    {
        return new FixPoint(fp.m_raw_value >> bits);
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

    public static FixPoint Distance(FixPoint x, FixPoint y)
    {
        return Sqrt(x * x + y * y);
    }

    public static FixPoint FastDistance(FixPoint x, FixPoint y)
    {
        //误差 < 8%
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
        //误差 < 0.02%
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
        //误差 < 0.02%
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

    public static FixPoint Tan(FixPoint radian)
    {
        FixPoint cos_value = Cos(radian);
        if (cos_value.m_raw_value == 0L)
            return FixPoint.MaxValue;
        FixPoint sin_value = Sin(radian);
        return sin_value / cos_value;
    }

    public static FixPoint Atan2(FixPoint y, FixPoint x)
    {
        //返回[0, FixPoint.TwoPi]
        //误差 < 0.005%
        long y1 = y.m_raw_value;
        long x1 = x.m_raw_value;
        if (x1 > 0)
        {
            if (y1 > 0)
            {
                if (y1 > x1)
                    return new FixPoint(HALF_PI - Atan2Table[(x1 << FRACTIONAL_PLACES) / y1]);
                else
                    return new FixPoint(Atan2Table[(y1 << FRACTIONAL_PLACES) / x1]);
            }
            else if (y1 < 0)
            {
                if (-y1 > x1)
                    return new FixPoint(ONE_AND_HALF_PI + Atan2Table[(x1 << FRACTIONAL_PLACES) / (-y1)]);
                else
                    return new FixPoint(TWO_PI - Atan2Table[((-y1) << FRACTIONAL_PLACES) / x1]);
            }
            else
            {
                return Zero;
            }
        }
        else if (x1 < 0)
        {
            if (y1 > 0)
            {
                if (y1 > -x1)
                    return new FixPoint(HALF_PI + Atan2Table[((-x1) << FRACTIONAL_PLACES) / y1]);
                else
                    return new FixPoint(PI - Atan2Table[(y1 << FRACTIONAL_PLACES) / (-x1)]);
            }
            else if (y1 < 0)
            {
                if (-y1 < -x1)
                    return new FixPoint(PI + Atan2Table[((-y1) << FRACTIONAL_PLACES) / (-x1)]);
                else
                    return new FixPoint(ONE_AND_HALF_PI - Atan2Table[((-x1) << FRACTIONAL_PLACES) / (-y1)]);
            }
            else
            {
                return Pi;
            }
        }
        else
        {
            if (y1 > 0)
            {
                return HalfPi;
            }
            else if (y1 < 0)
            {
                return OneAndHalfPi;
            }
            else
            {
                return Zero;
            }
        }
    }

    public static FixPoint Degree2Radian(FixPoint degree)
    {
        return degree * RadianPerDegree;
    }

    public static FixPoint Radian2Degree(FixPoint radian)
    {
        return radian * DegreePerRadian;
    }

    #region 方便函数
    public static FixPoint Min(FixPoint term1, FixPoint term2)
    {
        if (term2 < term1)
            return term2;
        return term1;
    }

    public static FixPoint Max(FixPoint term1, FixPoint term2)
    {
        if (term2 > term1)
            return term2;
        return term1;
    }

    public static FixPoint Clamp(FixPoint term, FixPoint min_value, FixPoint max_value)
    {
        if (term < min_value)
            return min_value;
        if (term > max_value)
            return max_value;
        return term;
    }
    #endregion

    #region 内部
    const int NUM_BITS = 64;
    const int FRACTIONAL_PLACES = 16;
    const long ONE = 1L << FRACTIONAL_PLACES;
    const long MAX_VALUE = long.MaxValue;
    const long MIN_VALUE = long.MinValue;
    //3.14159265358979323846264338327950288419716939937510
    const long QUARTER_PI = 51471L;
    const long HALF_PI = 102942L;
    const long PI = 205884L;
    const long ONE_AND_HALF_PI = 308826L;
    const long TWO_PI = 411768L;
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

    internal static void GenerateAtan2Table()
    {
        using (var writer = new StreamWriter("FixPointAtan2Table.cs"))
        {
            writer.Write(
@"partial struct FixPoint {
    public static readonly long[] Atan2Table = new[] {");
            int line_counter = 0;
            int atan2_table_size = (1 << FRACTIONAL_PLACES) + 1;
            double x = (double)(atan2_table_size);
            for (int i = 0; i < atan2_table_size; ++i)
            {
                if (line_counter++ % 8 == 0)
                {
                    writer.WriteLine();
                    writer.Write("        ");
                }
                double y = (double)i;
                double radian = Math.Atan2(y, x);
                long raw_value = ((FixPoint)radian).m_raw_value;
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

    static readonly char WhiteSpace = (char)1;
    static readonly char Error_____ = (char)2;
    static readonly char Digit_____ = (char)3;
    static readonly char Sign______ = (char)4;
    static readonly char Point_____ = (char)5;
    static readonly char[] ParseCodeTable = new[]{
        /*     0          1          2          3          4          5          6          7          8          9               */
        /*  0*/WhiteSpace,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,WhiteSpace,/*  0*/
        /* 10*/WhiteSpace,Error_____,Error_____,WhiteSpace,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 10*/
        /* 20*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 20*/
        /* 30*/Error_____,Error_____,WhiteSpace,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 30*/
        /* 40*/Error_____,Error_____,Error_____,Sign______,WhiteSpace,Sign______,Point_____,Error_____,Digit_____,Digit_____,/* 40*/
        /* 50*/Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Digit_____,Error_____,Error_____,/* 50*/
        /* 60*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 60*/
        /* 70*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 70*/
        /* 80*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 80*/
        /* 90*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/* 90*/
        /*100*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*100*/
        /*110*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*110*/
        /*120*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*120*/
        /*130*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*130*/
        /*140*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*140*/
        /*150*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*150*/
        /*160*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*160*/
        /*170*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*170*/
        /*180*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*180*/
        /*190*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*190*/
        /*200*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*200*/
        /*210*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*210*/
        /*220*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*220*/
        /*230*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*230*/
        /*240*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,Error_____,/*240*/
        /*250*/Error_____,Error_____,Error_____,Error_____,Error_____,Error_____                                             /*250*/
    };
    #endregion
}