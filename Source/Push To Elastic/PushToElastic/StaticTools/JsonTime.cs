using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToElastic.StaticTools
{
    static class JsonTime
    {
        public static string Now()
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            string month = LeftPadZero(now.Month.ToString());
            string day = LeftPadZero(now.Day.ToString());
            string hour = LeftPadZero(now.Hour.ToString());
            string minute = LeftPadZero(now.Minute.ToString());
            string second = LeftPadZero(now.Second.ToString());
            return year + "-" + month + "-" + day + "T" + hour + ":" + minute + ":" + second;
        }

        public static string Convert(DateTime dateTime)
        {
            string year = dateTime.Year.ToString();
            string month = LeftPadZero(dateTime.Month.ToString());
            string day = LeftPadZero(dateTime.Day.ToString());
            string hour = LeftPadZero(dateTime.Hour.ToString());
            string minute = LeftPadZero(dateTime.Minute.ToString());
            string second = LeftPadZero(dateTime.Second.ToString());
            return year + "-" + month + "-" + day + "T" + hour + ":" + minute + ":" + second;
        }

        private static string LeftPadZero(string input)
        {
            if(input.Length == 1)
            {
                return "0" + input;
            }
            return input;
        }

    }
}
