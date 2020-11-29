using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RealTimeBid.Core
{
    public class AdvertiserRepository
    {
        private Dictionary<string, DbAdvertiser> _cachedData;
        private Dictionary<string, ulong[]> _hourlyPrints;
        private readonly IDateTimeService _dateTimeService;
        private SpinLock _cachedDataSpinLock;
        private int currentHour;
        public AdvertiserRepository(List<DbAdvertiser> advertaisers, IDateTimeService dateTimeService)
        {
            _cachedData = new Dictionary<string, DbAdvertiser>(advertaisers.Count);
            _hourlyPrints = new Dictionary<string, ulong[]>(advertaisers.Count);
            foreach (var adv in advertaisers)
            {
                _cachedData.Add(adv.Id, adv);
                _hourlyPrints.Add(adv.Id, new ulong[24]);
                _hourlyPrints[adv.Id][0] = adv.CurrentHourlyPrints;
            }
            _dateTimeService = dateTimeService;
            currentHour = _dateTimeService.GetCurrentDateTime().Hour;
        }

        public void IncrementPrint(Advertiser adv, DateTime date)
        {
            ulong[] hourlyPrints = null;
            DbAdvertiser dbAdv = null;
            var lockTaken = false;

            _cachedDataSpinLock.Enter(ref lockTaken);
            if (_hourlyPrints.ContainsKey(adv.Id))
            {
                hourlyPrints = _hourlyPrints[adv.Id];
                dbAdv = _cachedData[adv.Id];
            }
            if (lockTaken) _cachedDataSpinLock.Exit();

            if (hourlyPrints != null)
            {
                var hourIndex = _dateTimeService.GetCurrentDateTime().Hour - date.Hour;
                if (hourIndex < 0)
                    hourIndex += 24;

                var lastModified = Interlocked.Increment(ref hourlyPrints[hourIndex]);
                if (hourIndex == 0) //current
                {
                    dbAdv.CurrentHourlyPrints = lastModified;
                }
            }
        }



        /// <summary>
        /// use this method to update prints in batch (for faster process)
        /// </summary>
        /// <param name="advPrints"></param>
        public void IncrementPrint(List<(Advertiser adv, DateTime date)> advPrints)
        {
            Dictionary<string, DbAdvertiser> advertisersToUpdate = new Dictionary<string, DbAdvertiser>();
            Dictionary<string, ulong[]> hoursToUpdate = new Dictionary<string, ulong[]>();

            var lockTaken = false;

            //thread safe references.
            _cachedDataSpinLock.Enter(ref lockTaken);
            foreach (var pair in advPrints)
            {
                if (_hourlyPrints.ContainsKey(pair.adv.Id) && !hoursToUpdate.ContainsKey(pair.adv.Id))
                {
                    hoursToUpdate.Add(pair.adv.Id, _hourlyPrints[pair.adv.Id]);
                    advertisersToUpdate.Add(pair.adv.Id, _cachedData[pair.adv.Id]);
                }
            }
            if (lockTaken) _cachedDataSpinLock.Exit();

            foreach (var pair in advPrints)
            {
                if (hoursToUpdate.ContainsKey(pair.adv.Id))
                {
                    var hourIndex = _dateTimeService.GetCurrentDateTime().Hour - pair.date.Hour;
                    if (hourIndex < 0)
                        hourIndex += 24;

                    var lastModified = Interlocked.Increment(ref hoursToUpdate[pair.adv.Id][hourIndex]);
                    if (hourIndex == 0) //current
                    {
                        advertisersToUpdate[pair.adv.Id].CurrentHourlyPrints = lastModified;
                    }
                }
            }
        }

        /// <summary>
        /// this method have to be called every period of time to flush cached data to db,
        /// will transform last 24 hour print count array to real datetime to persist/update prints per hour per day.
        /// </summary>
        public void FlushToDb()
        {

        }

        /// <summary>
        /// this method is called by 3rd party instance every short period of time to update hourly print array
        /// asumes it's regulary called
        /// </summary>
        public void UpdateHourlyPrints()
        {
            if(currentHour != _dateTimeService.GetCurrentDateTime().Hour)
            {
                currentHour = _dateTimeService.GetCurrentDateTime().Hour;
                //need array slide
                var lockTaken = false;
                _cachedDataSpinLock.Enter(ref lockTaken);
                var hourlyList = _hourlyPrints.Values.ToList();
                if(lockTaken) _cachedDataSpinLock.Exit();
                foreach (var item in hourlyList)
                {
                    for (int i = 23; i > 0; i--)
                    {
                        Interlocked.Exchange(ref item[i], Interlocked.Read(ref item[i-1]));
                    }
                    Interlocked.Exchange(ref item[0], 0);
                }

            }
        }


        /// <summary>
        /// Database have to be accessed through this class, so keep cache controlled.
        /// </summary>
        /// <param name="advData"></param>
        public void UpdateAdvertiser(DbAdvertiser advData)
        {
            var lockTaken = false;
            _cachedDataSpinLock.Enter(ref lockTaken);

            if (_cachedData.ContainsKey(advData.Id))
            {
                advData.CurrentHourlyPrints = _cachedData[advData.Id].CurrentHourlyPrints;
                _cachedData[advData.Id] = advData;
            }
            else
            {
                _cachedData[advData.Id] = advData;
                _hourlyPrints[advData.Id] = new ulong[24];
            }
            if (lockTaken) _cachedDataSpinLock.Exit();
        }


        /// <summary>
        /// returns current cached advertized
        /// </summary>
        /// <returns></returns>
        public List<DbAdvertiser> GetAvailableAdvertisers()
        {
            var lockTaken = false;
            _cachedDataSpinLock.Enter(ref lockTaken);
            var list = _cachedData.Values.ToList();
            if (lockTaken) _cachedDataSpinLock.Exit();
            return list;
        }
    }    
}
