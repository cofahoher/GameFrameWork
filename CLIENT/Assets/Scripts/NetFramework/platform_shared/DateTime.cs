using System;
using System.Collections;

namespace BaseUtil
{
    public class TimeUtil
    {
        public const int HOUR_AT_ZERO_TIME = 8;
        public const int ONE_HOUR_SECONDS = 3600;
        public const int ONEMINUTE_SECONDS = 60;
        public const int ONEDAY_SECONDS = 86400;	///< 1天的秒数
        public const int SECONDS_OFFSET = HOUR_AT_ZERO_TIME * ONE_HOUR_SECONDS;
        public const int ONEWEEK_SECONDS	= 604800;

        public static long ReadTimeStr(string time_str)
        {
            if (null == time_str)
            {
                return 0;
            }
            int hour = 0;
            int min = 0;
            int sec = 0;
            string[] str_arr = time_str.Split(':');
            if (str_arr.Length == 2)
            {
                int.TryParse(str_arr[0], out hour);
                int.TryParse(str_arr[1], out min);
            }
            else if (str_arr.Length == 3)
            {
                int.TryParse(str_arr[0], out hour);
                int.TryParse(str_arr[1], out min);
                int.TryParse(str_arr[2], out sec);
            }

            return ( hour * 3600 + min * 60 + sec );
        }

        public static long GetDayNum(long t)
        {
            long ret = (t + SECONDS_OFFSET) / ONEDAY_SECONDS;
            return ret;
        }

        public static bool IsSameDay(long t1, long t2)
        {
	        return(GetDayNum(t1) == GetDayNum(t2));
        }

        public static long GetDayStart(long t)
        {
            return (GetDayNum(t) * ONEDAY_SECONDS - SECONDS_OFFSET);
        }

        public static bool Contain(long last_time, long cur_time, long hms)
        {
            if (last_time > cur_time)
            {
                long tmp = last_time;
                last_time = cur_time;
                cur_time = tmp;
            }

            if ((cur_time - last_time) >= ONEDAY_SECONDS)
            { //超过1天了
                return (true);
            }

            long last_num = GetDayNum(last_time);
            long curr_num = GetDayNum(cur_time);
            long day = curr_num - last_num;

            bool result = false;
            if (day > 1)         //超过1天
            {
                result = true;
            }
            else if (day < 1)       //同一天
            {
                hms += GetDayStart(cur_time);
                result = ((last_time < hms) && (hms < cur_time));
            }
            else                //差一天
            {
                last_time -= GetDayStart(last_time);
                cur_time -= GetDayStart(cur_time);
                result = ((last_time < hms) || (cur_time > hms));
            }
            return (result);
        }

        public static bool IsWeekDay(long nowt, long reset, int wday)
        {
            time_t tnow = new time_t(nowt - reset);
            return ((int)tnow.dTime.DayOfWeek == wday);
        }

        public static long GetWeekStart(long ti)
        {
            time_t tm = new time_t(ti);
            int wday = (int)tm.dTime.DayOfWeek;
            if (wday == 0) wday = 7;
            long differ =   (wday - 1) * ONEDAY_SECONDS + 
                            tm.dTime.Hour * ONE_HOUR_SECONDS + 
                            tm.dTime.Minute * ONEMINUTE_SECONDS + 
                            tm.dTime.Second;
            long weekbegin = ti - differ;
            return ((weekbegin > 0) ? weekbegin : 0);
        }

        public static bool ContainWeek(long last, long curr, int wday, long time_reset)
        {
            if (last > curr)
            {
                long tmp = last;
                last = curr;
                curr = tmp;
            }

            if (wday == 0)
            {
                wday = 7;
            }

            long ti_start_last = GetWeekStart(last);
            long ti_start_cur = GetWeekStart(curr);

            ti_start_last += (wday - 1) * ONEDAY_SECONDS + time_reset;
            ti_start_cur += (wday - 1) * ONEDAY_SECONDS + time_reset;

            if (ti_start_last <= last)
            {
                ti_start_last += ONEWEEK_SECONDS;
            }

            if (ti_start_cur >= curr)
            {
                ti_start_cur -= ONEWEEK_SECONDS;
            }

            if (ti_start_cur < ti_start_last)
            {
                return false;
            }

            return true;
        }
    }
}
