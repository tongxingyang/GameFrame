using System;
using System.Globalization;
using System.Text;

namespace GameFrame
{
    /// <summary>
    /// 获取时间的表示形式
    /// </summary>
    public class TimeUtil
    {
        public const int MILLISECOND = 1000;
        public const int SECOND = 60;
        public const int MINUTE = 60;
        public const int HOUR = 24;

        public const int second = 1;
        public const int minute = 60 * second;
        public const int hour = 60 * minute;
        public const int day = 24 * hour;
        public const int week = 7 * day;
        public const int month = 30 * day;
        public const int year = 365 * day;

        static public string GetTimeShort(int time, int len = 1)
        {
            StringBuilder s = new StringBuilder();
            Action<int, string> act = (u, c) => {
                int num = time / u;
                s.Append(num);
                s.Append(c);
                time -= u * num;
                len--;
            };
            if (time >= year) act(year, "年");
            if (len > 0 && time >= month) act(month, "月");
            if (len > 0 && time >= week) act(week, "周");
            if (len > 0 && time >= day) act(day, "天");
            if (len > 0 && time > hour) act(hour, "时");
            if (len > 0 && time > minute) act(minute, "分");
            if (len > 0 && time > second) act(second, "秒");
            if (s.Length < 1) s.Append("0秒");
            return s.ToString();
        }

        private static DateTime _beginDateTime = new DateTime(1970, 1, 1, 0, 0, 0);

        public static string SecondToMinute(int time)
        {
            return time / SECOND + ":" + string.Format("{0:D2}", time % SECOND);
        }

        public static string MsToHourString(int ms) 
        {
            return SecondToHour(ms) + ":" + string.Format("{0:D2}", ms % (SECOND * MILLISECOND) / MINUTE);
        }

        public static int SecondToHour(int ms) 
        {
            return ms / 3600 / 1000;
        }

        public static int SecondToMinuteInt(int ms) 
        {
            return ms / 60 / 1000;
        }

        public static string SecondToHourString(int s)
        {
            return (s / 3600) + ":" + string.Format("{0:D2}", s % (SECOND * MINUTE) / MINUTE);
        }

        public static string SecondToHourMinSecString(int s)
        {
            return string.Format("{0:D2}",(s / 3600)) + ":" + string.Format("{0:D2}", s % 3600 / MINUTE) + ":" + string.Format("{0:D2}", s % 60);
        }

        public static long GetTime()
        {
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - _beginDateTime.Ticks);
            return (long)ts.TotalSeconds;
        }
        public static string GetTimeStr()
        {
            return GetTime().ToString();
        }
        public static string GetStrForTime(string str, string format)
        {
            return new DateTime(long.Parse(str) * 10000 * 1000 + _beginDateTime.Ticks).ToString(format);
        }
    }
}