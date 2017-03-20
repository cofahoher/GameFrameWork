namespace GB18030
{
    using System;
    using System.Reflection;

    internal class GB18030Source
    {
        private unsafe static readonly byte* gbx2uni;
        private static readonly int gbx2uniSize;
        private static readonly long gbxBase = FromGBXRaw(0x81, 0x30, 0x81, 0x30, false);
        private static readonly long gbxSuppBase = FromGBXRaw(0x90, 0x30, 0x81, 0x30, false);
        private static readonly GB18030Map[] ranges = new GB18030Map[] { new GB18030Map(0x452, 0x200f, FromGBXRaw(0x81, 0x30, 0xd3, 0x30, false), FromGBXRaw(0x81, 0x36, 0xa5, 0x31, false), false), new GB18030Map(0x2643, 0x2e80, FromGBXRaw(0x81, 0x37, 0xa8, 0x39, false), FromGBXRaw(0x81, 0x38, 0xfd, 0x38, false), false), new GB18030Map(0x361b, 0x3917, FromGBXRaw(130, 0x30, 0xa6, 0x33, false), FromGBXRaw(130, 0x30, 0xf2, 0x37, false), false), new GB18030Map(0x3ce1, 0x4055, FromGBXRaw(130, 0x31, 0xd4, 0x38, false), FromGBXRaw(130, 50, 0xaf, 50, false), false), new GB18030Map(0x4160, 0x4336, FromGBXRaw(130, 50, 0xc9, 0x37, false), FromGBXRaw(130, 50, 0xf8, 0x37, false), false), new GB18030Map(0x44d7, 0x464b, FromGBXRaw(130, 0x33, 0xa3, 0x39, false), FromGBXRaw(130, 0x33, 0xc9, 0x31, false), false), new GB18030Map(0x478e, 0x4946, FromGBXRaw(130, 0x33, 0xe8, 0x38, false), FromGBXRaw(130, 0x34, 150, 0x38, false), false), new GB18030Map(0x49b8, 0x4c76, FromGBXRaw(130, 0x34, 0xa1, 0x31, false), FromGBXRaw(130, 0x34, 0xe7, 0x33, false), false), new GB18030Map(0x4e00, 0x9fa5, 0L, 0L, true), new GB18030Map(0x9fa6, 0xd7ff, FromGBXRaw(130, 0x35, 0x8f, 0x33, false), FromGBXRaw(0x83, 0x36, 0xc7, 0x38, false), false), new GB18030Map(0xd800, 0xe76b, 0L, 0L, true), new GB18030Map(0xe865, 0xf92b, FromGBXRaw(0x83, 0x36, 0xd0, 0x30, false), FromGBXRaw(0x84, 0x30, 0x85, 0x34, false), false), new GB18030Map(0xfa2a, 0xfe2f, FromGBXRaw(0x84, 0x30, 0x9c, 0x38, false), FromGBXRaw(0x84, 0x31, 0x85, 0x37, false), false), new GB18030Map(0xffe6, 0xffff, FromGBXRaw(0x84, 0x31, 0xa2, 0x34, false), FromGBXRaw(0x84, 0x31, 0xa4, 0x39, false), false) };
        private unsafe static readonly byte* uni2gbx;
        private static readonly int uni2gbxSize;

        static unsafe GB18030Source()
        {
            //MethodInfo method = typeof(Assembly).GetMethod("GetManifestResourceInternal", BindingFlags.NonPublic | BindingFlags.Instance);
            //int num = 0;
            //Module module = null;
            //object[] parameters = new object[] { "encoding/gb18030.table", num, module };
            //IntPtr ptr = (IntPtr) method.Invoke(Assembly.GetExecutingAssembly(), parameters);
            byte[] data = null;
            CodePageLoader.Load("encoding/gb18030.table", out data);
            if(null == data) return;

            fixed (byte* c = &data[0])
            {
                IntPtr ptr = new IntPtr(c);
                if (ptr != IntPtr.Zero)
                {
                    gbx2uni = (byte*)ptr;
                    gbx2uniSize = (((gbx2uni[0] << 0x18) + (gbx2uni[1] << 0x10)) + (gbx2uni[2] << 8)) + gbx2uni[3];
                    gbx2uni += 4;
                    uni2gbx = gbx2uni + gbx2uniSize;
                    uni2gbxSize = (((uni2gbx[0] << 0x18) + (uni2gbx[1] << 0x10)) + (uni2gbx[2] << 8)) + uni2gbx[3];
                    uni2gbx += 4;
                }
            }            
        }

        private GB18030Source()
        {
        }

        public static long FromGBX(byte[] bytes, int start)
        {
            byte num = bytes[start];
            byte num2 = bytes[start + 1];
            byte num3 = bytes[start + 2];
            byte num4 = bytes[start + 3];
            if ((num < 0x81) || (num == 0xff))
            {
                return -1L;
            }
            if ((num2 < 0x30) || (num2 > 0x39))
            {
                return -2L;
            }
            if ((num3 < 0x81) || (num3 == 0xff))
            {
                return -3L;
            }
            if ((num4 < 0x30) || (num4 > 0x39))
            {
                return -4L;
            }
            if (num >= 0x90)
            {
                return FromGBXRaw(num, num2, num3, num4, true);
            }
            long num5 = FromGBXRaw(num, num2, num3, num4, false);
            long num6 = 0L;
            long num7 = 0L;
            for (int i = 0; i < ranges.Length; i++)
            {
                GB18030Map map = ranges[i];
                if (num5 < map.GStart)
                {
                    return (long) ToUcsRaw((int) ((num5 - num7) + num6));
                }
                if (num5 <= map.GEnd)
                {
                    return (((num5 - gbxBase) - map.GStart) + map.UStart);
                }
                if (map.GStart != 0)
                {
                    num6 += map.GStart - num7;
                    num7 = map.GEnd + 1L;
                }
            }
            throw new SystemException(string.Format("GB18030 INTERNAL ERROR (should not happen): GBX {0:x02} {1:x02} {2:x02} {3:x02}", new object[] { num, num2, num3, num4 }));
        }

        private static long FromGBXRaw(byte b1, byte b2, byte b3, byte b4, bool supp)
        {
            return (long) (((((((((b1 - (!supp ? 0x81 : 0x90)) * 10) + (b2 - 0x30)) * 0x7e) + (b3 - 0x81)) * 10) + b4) - 0x30) + (!supp ? 0 : 0x10000));
        }

        public static long FromUCS(int cp)
        {
            long num = 0L;
            long num2 = 0x80L;
            for (int i = 0; i < ranges.Length; i++)
            {
                GB18030Map map = ranges[i];
                if (cp < map.UStart)
                {
                    return ToGbxRaw((int) ((cp - num2) + num));
                }
                if (cp <= map.UEnd)
                {
                    return ((cp - map.UStart) + map.GStart);
                }
                if (map.GStart != 0)
                {
                    num += map.UStart - num2;
                    num2 = map.UEnd + 1;
                }
            }
            throw new SystemException(string.Format("GB18030 INTERNAL ERROR (should not happen): UCS {0:x06}", cp));
        }

        public static long FromUCSSurrogate(int cp)
        {
            return (cp + gbxSuppBase);
        }

        private static unsafe long ToGbxRaw(int idx)
        {
            if ((idx >= 0) && (((idx * 2) + 1) < uni2gbxSize))
            {
                return ((gbxBase + (uni2gbx[idx * 2] * 0x100)) + uni2gbx[(idx * 2) + 1]);
            }
            return -1L;
        }

        private static unsafe int ToUcsRaw(int idx)
        {
            return ((gbx2uni[idx * 2] * 0x100) + gbx2uni[(idx * 2) + 1]);
        }

        //public static unsafe void Unlinear(byte* bytes, long gbx)
        //{
        //    bytes[3] = (byte) ((gbx % 10L) + 0x30L);
        //    gbx /= 10L;
        //    bytes[2] = (byte) ((gbx % 0x7eL) + 0x81L);
        //    gbx /= 0x7eL;
        //    bytes[1] = (byte) ((gbx % 10L) + 0x30L);
        //    gbx /= 10L;
        //    bytes[0] = (byte) (gbx + 0x81L);
        //}

        //public static unsafe void Unlinear(byte[] bytes, int start, long gbx)
        //{
        //    fixed(byte* numPtr = ((bytes != null) && (bytes.Length != 0)) ? &(bytes[0]) : null))
        //    {
        //        Unlinear(numPtr + start, gbx);
        //        numPtr = null;
        //    }            
        //}

        private class GB18030Map
        {
            public readonly bool Dummy;
            public readonly long GEnd;
            public readonly long GStart;
            public readonly int UEnd;
            public readonly int UStart;

            public GB18030Map(int ustart, int uend, long gstart, long gend, bool dummy)
            {
                this.UStart = ustart;
                this.UEnd = uend;
                this.GStart = gstart;
                this.GEnd = gend;
                this.Dummy = dummy;
            }
        }
    }
}

