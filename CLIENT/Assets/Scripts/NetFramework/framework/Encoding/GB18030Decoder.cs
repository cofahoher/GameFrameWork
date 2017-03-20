namespace GB18030
{
    using System;

    internal class GB18030Decoder : DbcsEncoding.DbcsDecoder
    {
        private static DbcsConvert gb2312 = DbcsConvert.Gb2312;

        public GB18030Decoder() : base(null)
        {
        }

        public virtual char[] GetChars(byte[] bytes, int index, int count)
        {
            char[] chars = new char[this.GetCharCount(bytes, index, count)];
            this.GetChars(bytes, index, count, chars, 0);
            return chars;
        }

        public override int GetCharCount(byte[] bytes, int start, int len)
        {
            base.CheckRange(bytes, start, len);
            int num = start + len;
            int num2 = 0;
            while (start < num)
            {
                if (bytes[start] < 0x80)
                {
                    num2++;
                    start++;
                }
                else
                {
                    if (bytes[start] == 0x80)
                    {
                        num2++;
                        start++;
                        continue;
                    }
                    if (bytes[start] == 0xff)
                    {
                        num2++;
                        start++;
                        continue;
                    }
                    if ((start + 1) >= num)
                    {
                        num2++;
                        return num2;
                    }
                    byte num3 = bytes[start + 1];
                    switch (num3)
                    {
                        case 0x7f:
                        case 0xff:
                        {
                            num2++;
                            start += 2;
                            continue;
                        }
                    }
                    if ((0x30 <= num3) && (num3 <= 0x39))
                    {
                        if ((start + 3) >= num)
                        {
                            return (num2 + (((start + 3) != num) ? 2 : 3));
                        }
                        long num4 = GB18030Source.FromGBX(bytes, start);
                        if (num4 < 0L)
                        {
                            num2++;
                            start -= (int) num4;
                        }
                        else if (num4 >= 0x10000L)
                        {
                            num2 += 2;
                            start += 4;
                        }
                        else
                        {
                            num2++;
                            start += 4;
                        }
                        continue;
                    }
                    start += 2;
                    num2++;
                }
            }
            return num2;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            base.CheckRange(bytes, byteIndex, byteCount, chars, charIndex);
            int num = byteIndex + byteCount;
            int num2 = charIndex;
            while (byteIndex < num)
            {
                if (bytes[byteIndex] < 0x80)
                {
                    chars[charIndex++] = (char) bytes[byteIndex++];
                }
                else
                {
                    if (bytes[byteIndex] == 0x80)
                    {
                        chars[charIndex++] = '€';
                        byteIndex++;
                        continue;
                    }
                    if (bytes[byteIndex] == 0xff)
                    {
                        chars[charIndex++] = '?';
                        byteIndex++;
                        continue;
                    }
                    if ((byteIndex + 1) >= num)
                    {
                        break;
                    }
                    byte num3 = bytes[byteIndex + 1];
                    if ((num3 == 0x7f) || (num3 == 0xff))
                    {
                        chars[charIndex++] = '?';
                        byteIndex += 2;
                        continue;
                    }
                    if ((0x30 <= num3) && (num3 <= 0x39))
                    {
                        if ((byteIndex + 3) >= num)
                        {
                            break;
                        }
                        long num4 = GB18030Source.FromGBX(bytes, byteIndex);
                        if (num4 < 0L)
                        {
                            chars[charIndex++] = '?';
                            byteIndex -= (int) num4;
                        }
                        else if (num4 >= 0x10000L)
                        {
                            num4 -= 0x10000L;
                            chars[charIndex++] = (char) ((ushort) ((num4 / 0x400L) + 0xd800L));
                            chars[charIndex++] = (char) ((ushort) ((num4 % 0x400L) + 0xdc00L));
                            byteIndex += 4;
                        }
                        else
                        {
                            chars[charIndex++] = (char) ((ushort) num4);
                            byteIndex += 4;
                        }
                        continue;
                    }
                    byte num5 = bytes[byteIndex];
                    int index = ((((num5 - 0x81) * 0xbf) + num3) - 0x40) * 2;
                    char ch = ((index >= 0) && (index < gb2312.n2u.Length)) ? ((char) (gb2312.n2u[index] + (gb2312.n2u[index + 1] * 0x100))) : '\0';
                    if (ch == '\0')
                    {
                        chars[charIndex++] = '?';
                    }
                    else
                    {
                        chars[charIndex++] = ch;
                    }
                    byteIndex += 2;
                }
            }
            return (charIndex - num2);
        }
    }
}

