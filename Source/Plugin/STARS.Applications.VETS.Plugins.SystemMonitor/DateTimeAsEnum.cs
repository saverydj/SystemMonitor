using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STARS.Applications.VETS.Plugins.SystemMonitor
{
    public class DateTimeAsEnum
    {
        public int Year = 0;
        public int Month = 0;
        public int Day = 0;
        public int Hour = 0;
        public int Minute = 0;
        public int Second = 0;

        public DateTimeAsEnum(DateTime dateTime)
        {
            SetDateTime(dateTime);
        }

        public void SetDateTime(DateTime datetime)
        {
            Year = datetime.Year;
            Month = datetime.Month;
            Day = datetime.Day;
            Hour = datetime.Hour;
            Minute = datetime.Minute;
            Second = datetime.Second;
        }

        public string GetStringFormattedDateTime()
        {
            string amPm = "AM";
            string amPmHour = Hour.ToString();

            if (Hour == 0) amPmHour = "12";
            if (Hour > 12)
            {
                amPmHour = (Hour - 12).ToString();
                amPm = "PM";
            }

            return Month + "/" + Day + "/" + Year + " " + amPmHour + ":" + Minute + ":" + Second + " " + amPm;
        }
    }
}
