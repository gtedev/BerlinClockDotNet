using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BerlinClock
{
    public class TimeConverter : ITimeConverter
    {
        public string convertTime(string aTime)
        {
            int hoursInt, minutesInt, secondsInt;
            var berlinClockManager = new BerlinClockManager();

            #region Parsing

            if (!berlinClockManager.isParseTimeStringOk(aTime, out hoursInt, out minutesInt, out secondsInt))
            {
                throw new Exception("aTime is not on the right format, it has to be HH:mm:ss with numbers");
            }

            #endregion

            #region Range Validity

            if (!berlinClockManager.isDateRangeValid(hoursInt, minutesInt, secondsInt))
            {
                throw new Exception("Hours, Minutes or Seconds does not respect time numbers range (Example: 0 <= Hours <= 24");
            }

            #endregion

            var lightingSchemeString = berlinClockManager.GetLightingSchemeString(hoursInt, minutesInt, secondsInt);

            return lightingSchemeString;
        }

        #region BerlinClockManager

        public class BerlinClockManager
        {
            public string GetLightingSchemeString(int hours, int minutes, int seconds)
            {
                // Example of result = "Y\r\nOOOO\r\nOOOO\r\nOOOOOOOOOOO\r\nOOOO"

                var secondLampString = "Y";
                var fiveHourLampString = new String('O', 4);
                var oneHourLampString = new String('O', 4);
                var fiveMinuteLampString = new String('O', 11);
                var oneMinuteLampString = new String('O', 4);

                BerlinClockLightingScheme lightingScheme = GetLightingScheme(hours, minutes, seconds);


                Func<int, char, string, string> generateSwitchOnLampString = (howManyToSwitchOn, switchOnChar, lampString) =>
                {
                    var lampSwitchedOnString = new String(switchOnChar, howManyToSwitchOn);
                    var result = lampSwitchedOnString + lampString.Remove(0, howManyToSwitchOn);

                    return result;
                };

                #region Seconds

                secondLampString = lightingScheme.IsSecondLampSwitchOn ? "Y" : "O";

                #endregion

                #region Hours

                fiveHourLampString = generateSwitchOnLampString(lightingScheme.FiveHourLampToSwitchOnNumber, 'R', fiveHourLampString);
                oneHourLampString = generateSwitchOnLampString(lightingScheme.OneHourLampToSwitchOnNumber, 'R', oneHourLampString);

                #endregion

                #region Minutes

                fiveMinuteLampString = generateSwitchOnLampString(lightingScheme.FiveMinuteLampToSwitchOnNumber, 'Y', fiveMinuteLampString);

                for (int i = 1; i <= lightingScheme.QuarterEndedNumber; i++)
                {
                    var quarterIndex = (3 * i) - 1;
                    StringBuilder tempString = new StringBuilder(fiveMinuteLampString);
                    tempString[quarterIndex] = 'R';
                    fiveMinuteLampString = tempString.ToString();
                }

                oneMinuteLampString = generateSwitchOnLampString(lightingScheme.OneMinuteLampToSwitchOnNumber, 'Y', oneMinuteLampString);

                #endregion

                return secondLampString + "\r\n" +
                       fiveHourLampString + "\r\n" +
                       oneHourLampString + "\r\n" +
                       fiveMinuteLampString + "\r\n" +
                       oneMinuteLampString;
            }

            public BerlinClockLightingScheme GetLightingScheme(int hours, int minutes, int seconds)
            {
                var lightingScheme = new BerlinClockLightingScheme
                {
                    IsSecondLampSwitchOn = seconds % 2 == 0,
                    FiveHourLampToSwitchOnNumber = hours / 5,
                    OneHourLampToSwitchOnNumber = hours % 5,
                    FiveMinuteLampToSwitchOnNumber = minutes / 5,
                    QuarterEndedNumber = minutes / 15,
                    OneMinuteLampToSwitchOnNumber = minutes % 5
                };

                return lightingScheme;
            }

            public bool isDateRangeValid(int hours, int minutes, int seconds)
            {
                return (hours >= 0 && hours <= 24) &&
                       (minutes >= 0 && minutes <= 59) &&
                       (minutes >= 0 && minutes <= 59);
            }

            public bool isParseTimeStringOk(string timeString, out int hours, out int minutes, out int seconds)
            {
                hours = -1;
                minutes = -1;
                seconds = -1;

                var timeSplitted = timeString.Split(':');
                if (timeSplitted.Count() != 3)
                {
                    return false;
                }

                var isParseHoursOk = Int32.TryParse(timeSplitted[0], out hours);
                var isParseMinutesOk = Int32.TryParse(timeSplitted[1], out minutes);
                var isParseSecondsOk = Int32.TryParse(timeSplitted[2], out seconds);

                return isParseHoursOk && isParseMinutesOk && isParseSecondsOk;
            }
        }

        public class BerlinClockLightingScheme
        {
            public bool IsSecondLampSwitchOn { get; set; }

            public int FiveHourLampToSwitchOnNumber { get; set; }
            public int OneHourLampToSwitchOnNumber { get; set; }

            public int FiveMinuteLampToSwitchOnNumber { get; set; }
            public int QuarterEndedNumber { get; set; }

            public int OneMinuteLampToSwitchOnNumber { get; set; }
        }

        #endregion
    }
}
