namespace GB18030
{
    //using I18N.Common;
    using System;
    using System.Text;

    [Serializable]
    internal abstract class DbcsEncoding : Encoding
    {
        internal class Strings
        {
            internal static string GetString(string str)
            {
                return str;
            }
        }
        internal abstract class DbcsDecoder : System.Text.Decoder
        {
            protected DbcsConvert convert;

            public DbcsDecoder(DbcsConvert convert)
            {
                this.convert = convert;
            }

            internal void CheckRange(byte[] bytes, int index, int count)
            {
                if (bytes == null)
                {
                    throw new ArgumentNullException("bytes");
                }
                if ((index < 0) || (index > bytes.Length))
                {
                    throw new ArgumentOutOfRangeException("index", Strings.GetString("ArgRange_Array"));
                }
                if ((count < 0) || (count > (bytes.Length - index)))
                {
                    throw new ArgumentOutOfRangeException("count", Strings.GetString("ArgRange_Array"));
                }
            }

            internal void CheckRange(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            {
                if (bytes == null)
                {
                    throw new ArgumentNullException("bytes");
                }
                if (chars == null)
                {
                    throw new ArgumentNullException("chars");
                }
                if ((byteIndex < 0) || (byteIndex > bytes.Length))
                {
                    throw new ArgumentOutOfRangeException("byteIndex", Strings.GetString("ArgRange_Array"));
                }
                if ((byteCount < 0) || ((byteIndex + byteCount) > bytes.Length))
                {
                    throw new ArgumentOutOfRangeException("byteCount", Strings.GetString("ArgRange_Array"));
                }
                if ((charIndex < 0) || (charIndex > chars.Length))
                {
                    throw new ArgumentOutOfRangeException("charIndex", Strings.GetString("ArgRange_Array"));
                }
            }
        }
    }
}

