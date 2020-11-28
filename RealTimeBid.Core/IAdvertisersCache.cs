using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RealTimeBid.Core
{
    /// <summary>
    /// Serves current advertisers with data prepared for fast tag check
    /// </summary>
    public interface IAdvertisersCache
    {
        List<Advertiser> GetCurrentAdvertisers();
        void Update(List<DbAdvertiser> advertisers);
    }


    public class AdvertiserCache : IAdvertisersCache
    {
        public AdvertiserCache(IDateTimeService dateTimeService)
        {
            this._dateTimeService = dateTimeService;
        }

        private List<Advertiser> _currentAdvertisers = new List<Advertiser>();

        SpinLock _currentAdvertisersLock;
        private readonly IDateTimeService _dateTimeService;

        public List<Advertiser> GetCurrentAdvertisers()
        {
            var lockTaken = false;
            _currentAdvertisersLock.Enter(ref lockTaken);
            var list = _currentAdvertisers;
            if (lockTaken) _currentAdvertisersLock.Exit();
            return list;
        }

        /// <summary>
        /// this method is called by 3rd party instance repository.GetCurrentAdvertisers() as parameter
        /// updates current cache
        /// </summary>
        /// <param name="dbAdvertisers"></param>
        public void Update(List<DbAdvertiser> dbAdvertisers)
        {
            var list = new List<Advertiser>();

            foreach (var dbAdvertiser in dbAdvertisers)
            {
                var adv = Build(dbAdvertiser, _dateTimeService.GetCurrentDateTime());
                if (adv != null)
                    list.Add(adv);
            }
            list = list.OrderByDescending(x => x.CPM).ToList();

            //update cache by replacing list
            var lockTaken = false;
            _currentAdvertisersLock.Enter(ref lockTaken);
            _currentAdvertisers = list;
            if (lockTaken) _currentAdvertisersLock.Exit();
        }

        public Advertiser Build(DbAdvertiser repoAdv, DateTime dateTime)
        {
            var adv = new Advertiser();
            //Discard disabled
            if (!repoAdv.Enabled)
                return null;
            //discard hitted HourlyBudget
            if ((repoAdv.CurrentHourlyPrints / (decimal)1000) * repoAdv.CPM >= repoAdv.MaxHourlyBudget)
                return null;
            //Skip not available
            if (repoAdv.WorkingCalendar != null)
            {
                if (!repoAdv.WorkingCalendar.ContainsKey((byte)dateTime.DayOfWeek))
                    return null;
                if (!repoAdv.WorkingCalendar[(byte)dateTime.DayOfWeek].Contains(dateTime.Hour))
                    return null;
            }

            //optional Tags are not needed

            adv.RequiredTagsHashSetsFull = new HashSet<Tag>();
            for (int i = 0; i < repoAdv.RequiredTags.Count; i++)
            {
                adv.RequiredTagsHashSetsFull.Add(repoAdv.RequiredTags[i]);
            }
            adv.RequiredTagsCount = repoAdv.RequiredTags.Count;

            adv.RejectedTagsHashSetsFull = new HashSet<Tag>();
            for (int i = 0; i < repoAdv.RejectedTags.Count; i++)
            {
                adv.RejectedTagsHashSetsFull.Add(repoAdv.RejectedTags[i]);
            }

            adv.RequiredTagsHashSetsFull.TrimExcess();
            adv.RejectedTagsHashSetsFull.TrimExcess();

            adv.CPM = repoAdv.CPM;
            adv.Id = repoAdv.Id;
            return adv;
        }
    }

}
