using System;
using System.Collections;

namespace BaseUtil
{
    public struct TimeConstant
    {
        public static long sTicksFromStandard = 621356256000000000L;//从1970年开始的tick数        
        public static long sTicksPerSecond = 10000000L;//每秒多少tick
        public static long OneDaySecond = 86400L;
        public static long OneHourSecond = 3600L;
        public static long OneMinuteSecond = 60L;
    }
    public class time_t
    {        
        public DateTime dTime;

        public time_t()
        {
            dTime = new DateTime(0);
        }
        public time_t(long time)
        {
            dTime = new DateTime(TimeConstant.sTicksFromStandard + time * TimeConstant.sTicksPerSecond);
        }

        public time_t(DateTime dtime)
        {
            dTime = dtime;
        }

        public long AsLong()
        {
            return (dTime.Ticks - TimeConstant.sTicksFromStandard) / TimeConstant.sTicksPerSecond;
        }

        public void SetTime(DateTime dtime)
        {
            dTime = dtime;
        }
        public void Set(time_t time)
        {
            Set( time.AsLong() );
        }
        public void Set(long time)
        {
            dTime = new DateTime(TimeConstant.sTicksFromStandard + time * TimeConstant.sTicksPerSecond);
        }
        static public long Diff(time_t large, time_t little)
        {
            return (large.AsLong() - little.AsLong());
        }
    }
}
