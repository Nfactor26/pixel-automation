using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pixel.Automation.Web.Portal.Helpers
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Get the name of last n months ( up to last 12 months from current month)
        /// </summary>
        /// <param name="n">number of months</param>
        /// <returns></returns>
        public static IEnumerable<string> GetLastNMonths(int n)
        {
            if(n > 12)
            {
                throw new ArgumentException("Number of months can't be greater then 12");
            }

            int currentYear = DateTime.Now.Year;
            int currentMonthOfYear = DateTime.Now.Month;
            List<string> monthsSoFar = new List<string>();
            for (int i = 1; i <= n; i++)
            {
                monthsSoFar.Add($"{DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(currentMonthOfYear)}, {currentYear}");
                currentMonthOfYear--;
                if (currentMonthOfYear == 0)
                {
                    currentMonthOfYear = 12;
                    currentYear--;
                }
            }
            monthsSoFar.Reverse();
            return monthsSoFar;
        }

        /// <summary>
        /// Get the DateTime n months before current DateTime
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static DateTime NMonthsBefore(int n)
        {
            if (n > 12)
            {
                throw new ArgumentException("Number of months can't be greater then 12");
            }

            int currentYear = DateTime.Now.Year;
            int currentMonthOfYear = DateTime.Now.Month;

            int numberOfDays = DateTime.Now.Day;
            while(n > 1)
            {
                currentMonthOfYear--;
                if(currentMonthOfYear == 12)
                {
                    currentYear--;
                }
                numberOfDays += DateTime.DaysInMonth(currentYear, currentMonthOfYear);
                n--;
            }

            return DateTime.Now.Subtract(TimeSpan.FromDays(numberOfDays)).ToUniversalTime();

        }
    }
}
