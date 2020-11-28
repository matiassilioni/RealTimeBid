using System.Collections.Generic;

namespace RealTimeBid.Core
{
    public class DbAdvertiser
    {
        public string Id { get; set; }
        public decimal CPM { get; set; }
        public bool Enabled { get; set; }
        public Dictionary<byte, List<int>> WorkingCalendar { get; set; }
        public List<Tag> OptionalTags { get; set; }
        public List<Tag> RequiredTags { get; set; }
        public List<Tag> RejectedTags { get; set; }
        public decimal MaxHourlyBudget { get; set; }
        private ulong currentHourlyPrints;

        public ulong CurrentHourlyPrints
        {
            get
            {
                return System.Threading.Interlocked.Read(ref currentHourlyPrints);
            }
            set
            {
                System.Threading.Interlocked.Exchange(ref currentHourlyPrints, value);
            }
        }
    }

    public class WorkingHoursDay
    {
        public List<int> Hours { get; set; }
    }
}
