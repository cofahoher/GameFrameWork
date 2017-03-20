namespace GB18030
{
    //using I18N.Common;
    using System;
    using System.IO;
    using System.Reflection;
    internal sealed class CodePageLoader
    {
        internal static Stream Load(string name, out Stream stream)
        {
            #if CONSOLE_CLIENT            
            stream = new FileStream(name+".bytes", FileMode.Open, FileAccess.Read);
            #else
            stream = new MemoryStream(UnityEngine.Resources.Load<UnityEngine.TextAsset>(name).bytes);
            #endif
            return stream;
        }
        internal static void Load(string name, out byte[] result)
        {
            result = null;
            #if CONSOLE_CLIENT     
            Stream stream = null;
            using (stream = Load(name, out stream))
            {
                if (0 >= stream.Length) return;
                result = new byte[stream.Length];
                stream.Read(result, 0, result.Length);
            }
            #else
            result = UnityEngine.Resources.Load<UnityEngine.TextAsset>(name).bytes;
            #endif
        }
    }
    internal sealed class CodeTable : IDisposable
    {
        private Stream stream;

        public CodeTable(string name)
        {
            CodePageLoader.Load(name, out stream);
            if (this.stream == null)
            {
                throw new NotSupportedException(name);
            }
        }

        public void Dispose()
        {
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream = null;
            }
        }

        public byte[] GetSection(int num)
        {
            if (this.stream != null)
            {
                long num2 = 0L;
                long length = this.stream.Length;
                byte[] buffer = new byte[8];
                while ((num2 + 8L) <= length)
                {
                    this.stream.Position = num2;
                    if (this.stream.Read(buffer, 0, 8) != 8)
                    {
                        break;
                    }
                    int num4 = ((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18);
                    int count = ((buffer[4] | (buffer[5] << 8)) | (buffer[6] << 0x10)) | (buffer[7] << 0x18);
                    if (num4 == num)
                    {
                        byte[] buffer2 = new byte[count];
                        if (this.stream.Read(buffer2, 0, count) == count)
                        {
                            return buffer2;
                        }
                        break;
                    }
                    num2 += 8 + count;
                }
            }
            return null;
        }
    }
}

