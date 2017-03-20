namespace GB18030
{
    using System;

    internal class DbcsConvert
    {
        //internal static readonly DbcsConvert Big5 = new DbcsConvert("big5.table");
        internal static readonly DbcsConvert Gb2312 = new DbcsConvert("encoding/gb2312.table");
        //internal static readonly DbcsConvert KS = new DbcsConvert("ks.table");
        public byte[] n2u;
        public byte[] u2n;

        internal DbcsConvert(string fileName)
        {
            using (CodeTable table = new CodeTable(fileName))
            {
                this.n2u = table.GetSection(1);
                this.u2n = table.GetSection(2);
            }
        }
    }
}

