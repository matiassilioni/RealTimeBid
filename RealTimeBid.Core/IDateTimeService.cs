using System;

namespace RealTimeBid.Core
{
    public interface IDateTimeService
    {
        DateTime GetCurrentDateTime();
    }
    public class DateTimeService : IDateTimeService
    {
        public DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}
