using NSubstitute;
using RealTimeBid.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace RealTimeBid.Tests
{
    public class BetTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public BetTests(ITestOutputHelper testOutput)
        {
            this.testOutputHelper = testOutput;
        }

        [Fact]
        public void TwoEqualAdvertisersWinsBestBet()
        {

            var wednesday = new DateTime(2020, 11, 25, 12, 0, 0); //wed noon
            var dbAdvertisers = new List<DbAdvertiser>();
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 10,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = null,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 1,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = null,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });

            var printNotificationService = Substitute.For<IPrintNotificationService>();
            var datetimeService = Substitute.For<IDateTimeService>();
            datetimeService.GetCurrentDateTime().Returns(DateTime.Now);

            var advertiserRepository = new AdvertiserRepository(dbAdvertisers, datetimeService);

            var advertiserCache = new AdvertiserCache(datetimeService);

            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());

            var rtService = new RtBidService(advertiserCache, printNotificationService, datetimeService);

            var bid = new Bid
            {
                ID = Guid.NewGuid().ToString(),
                Tags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } }
            };

            Advertiser winner = null;
            winner = rtService.Advertise(bid);

            Assert.Equal(dbAdvertisers[0].Id, winner.Id);
        }

        [Fact]
        public void TwoAdvertisersWinsOnlyWithRequired()
        {

            var wednesday = new DateTime(2020, 11, 25, 12, 0, 0); //wed noon
            var dbAdvertisers = new List<DbAdvertiser>();
            //losser
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 10,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = null,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });
            //winner
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 1,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }},
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = null,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });

            var printNotificationService = Substitute.For<IPrintNotificationService>();
            var datetimeService = Substitute.For<IDateTimeService>();
            datetimeService.GetCurrentDateTime().Returns(DateTime.Now);

            var advertiserRepository = new AdvertiserRepository(dbAdvertisers, datetimeService);

            var advertiserCache = new AdvertiserCache(datetimeService);

            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());

            var rtService = new RtBidService(advertiserCache, printNotificationService, datetimeService);

            var bid = new Bid
            {
                ID = Guid.NewGuid().ToString(),
                Tags = new List<Tag> { new Tag { Key = "key1", Value = "value1" } }
            };

            Advertiser winner = null;
            winner = rtService.Advertise(bid);

            Assert.Equal(dbAdvertisers[1].Id, winner.Id);
        }

        [Fact]
        public void TwoAdvertisersWinsWithNoRejectedValue()
        {

            var wednesday = new DateTime(2020, 11, 25, 12, 0, 0); //wed noon
            var dbAdvertisers = new List<DbAdvertiser>();
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 10,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value2" } },
                WorkingCalendar = null,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });

            //winner
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 1,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" } },
                WorkingCalendar = null,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });

            var printNotificationService = Substitute.For<IPrintNotificationService>();
            var datetimeService = Substitute.For<IDateTimeService>();
            datetimeService.GetCurrentDateTime().Returns(DateTime.Now);

            var advertiserRepository = new AdvertiserRepository(dbAdvertisers, datetimeService);

            var advertiserCache = new AdvertiserCache(datetimeService);

            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());

            var rtService = new RtBidService(advertiserCache, printNotificationService, datetimeService);

            var bid = new Bid
            {
                ID = Guid.NewGuid().ToString(),
                Tags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } }
            };

            Advertiser winner = null;
            winner = rtService.Advertise(bid);

            Assert.Equal(dbAdvertisers[1].Id, winner.Id);
        }

        [Fact]
        public void TwoAdvertisersSameByWorkingDateWinsBestBet()
        {

            var wednesday = new DateTime(2020, 11, 25, 12, 0, 0); //wed noon

            var workingCalendarWednesday = new Dictionary<byte, List<int>>();
            workingCalendarWednesday.Add((byte)DayOfWeek.Wednesday, Enumerable.Range(12, 23).ToList());

            var dbAdvertisers = new List<DbAdvertiser>();
            //winner
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 10,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = null,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 1,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = workingCalendarWednesday,
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });

            var printNotificationService = Substitute.For<IPrintNotificationService>();
            var datetimeService = Substitute.For<IDateTimeService>();

            //returns wednesday
            datetimeService.GetCurrentDateTime().Returns(wednesday);

            var advertiserRepository = new AdvertiserRepository(dbAdvertisers, datetimeService);

            var advertiserCache = new AdvertiserCache(datetimeService);

            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());

            var rtService = new RtBidService(advertiserCache, printNotificationService, datetimeService);

            var bid = new Bid
            {
                ID = Guid.NewGuid().ToString(),
                Tags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } }
            };

            Advertiser winner = null;
            winner = rtService.Advertise(bid);

            Assert.Equal(dbAdvertisers[0].Id, winner.Id);
        }

        [Fact]
        public void TwoAdvertisersWinsWorkingDate()
        {

            var wednesday = new DateTime(2020, 11, 25, 12, 0, 0); //wed noon

            var workingCalendarWednesdayAfternoon = new Dictionary<byte, List<int>>();
            workingCalendarWednesdayAfternoon.Add((byte)DayOfWeek.Wednesday, Enumerable.Range(12, 23).ToList());//afternoon

            var workingCalendarWednesdayMorning = new Dictionary<byte, List<int>>();
            workingCalendarWednesdayMorning.Add((byte)DayOfWeek.Wednesday, Enumerable.Range(0, 11).ToList());//morning

            var dbAdvertisers = new List<DbAdvertiser>();
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 10,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = workingCalendarWednesdayMorning,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });
            //winner
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 1,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = workingCalendarWednesdayAfternoon,
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });

            var printNotificationService = Substitute.For<IPrintNotificationService>();
            var datetimeService = Substitute.For<IDateTimeService>();

            //returns wednesday
            datetimeService.GetCurrentDateTime().Returns(wednesday);

            var advertiserRepository = new AdvertiserRepository(dbAdvertisers, datetimeService);

            var advertiserCache = new AdvertiserCache(datetimeService);

            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());

            var rtService = new RtBidService(advertiserCache, printNotificationService, datetimeService);

            var bid = new Bid
            {
                ID = Guid.NewGuid().ToString(),
                Tags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } }
            };

            Advertiser winner = null;
            winner = rtService.Advertise(bid);

            Assert.Equal(dbAdvertisers[1].Id, winner.Id);
        }


        [Fact]
        public void TwoAdvertisersWinsWorkingDateThenWinsWhenHourChanges()
        {
            var wednesdayNoon = new DateTime(2020, 11, 25, 12, 0, 0); //wed noon
            var wednesdayMorning = new DateTime(2020, 11, 25, 7, 0, 0); //wed morning

            var workingCalendarWednesdayMorning = new Dictionary<byte, List<int>>();
            workingCalendarWednesdayMorning.Add((byte)DayOfWeek.Wednesday, Enumerable.Range(0, 11).ToList());//morning

            var workingCalendarWednesdayAfternoon = new Dictionary<byte, List<int>>();
            workingCalendarWednesdayAfternoon.Add((byte)DayOfWeek.Wednesday, Enumerable.Range(12, 23).ToList());//afternoon

            var dbAdvertisers = new List<DbAdvertiser>();
            
            //wins 1st time
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 10,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = workingCalendarWednesdayMorning,
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });
            //wins 2nd time
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 1,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = workingCalendarWednesdayAfternoon,
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });

            var printNotificationService = Substitute.For<IPrintNotificationService>();
            var datetimeService = Substitute.For<IDateTimeService>();

            //returns wednesdayMorning
            datetimeService.GetCurrentDateTime().Returns(wednesdayMorning);

            var advertiserRepository = new AdvertiserRepository(dbAdvertisers, datetimeService);

            var advertiserCache = new AdvertiserCache(datetimeService);

            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());

            var rtService = new RtBidService(advertiserCache, printNotificationService, datetimeService);

            var bid = new Bid
            {
                ID = Guid.NewGuid().ToString(),
                Tags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } }
            };

            Advertiser winner = null;
            winner = rtService.Advertise(bid);

            Assert.Equal(dbAdvertisers[0].Id, winner.Id);

            //from notification service, print takes message from queue and calls:
            advertiserRepository.IncrementPrint(winner, wednesdayMorning);

            //hour changes
            datetimeService.GetCurrentDateTime().Returns(wednesdayNoon);

            //from 3rd party controlling this node calls periodically (30 secs) to update cache
            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());
            //also periodically called by controller
            advertiserRepository.UpdateHourlyPrints();

            winner = rtService.Advertise(bid);
            //next advertiser is different
            
            Assert.Equal(dbAdvertisers[1].Id, winner.Id);

        }


        [Fact]
        public void ExeedsBudget()
        {
            var wednesday = new DateTime(2020, 11, 25, 12, 0, 0); //wed noon
            var dbAdvertisers = new List<DbAdvertiser>();
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 10,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = null,//no date restrictions
                CurrentHourlyPrints = 999,//needs 1 print to exceed
                MaxHourlyBudget = 10, //just 1 print
            });
            dbAdvertisers.Add(new DbAdvertiser
            {
                CPM = 1,
                Enabled = true,
                Id = Guid.NewGuid().ToString(),
                RequiredTags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } },
                RejectedTags = new List<Tag> { new Tag { Key = "key1", Value = "value2" }, new Tag { Key = "key2", Value = "value1" }, },
                WorkingCalendar = null,//no date restrictions
                MaxHourlyBudget = decimal.MaxValue,//no budgetRestrictions
            });

            var printNotificationService = Substitute.For<IPrintNotificationService>();
            var datetimeService = Substitute.For<IDateTimeService>();
            datetimeService.GetCurrentDateTime().Returns(wednesday);

            var advertiserRepository = new AdvertiserRepository(dbAdvertisers, datetimeService);

            var advertiserCache = new AdvertiserCache(datetimeService);

            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());

            var rtService = new RtBidService(advertiserCache, printNotificationService, datetimeService);

            var bid = new Bid
            {
                ID = Guid.NewGuid().ToString(),
                Tags = new List<Tag> { new Tag { Key = "key1", Value = "value1" }, new Tag { Key = "key2", Value = "value2" } }
            };

            Advertiser winner = null;
            winner = rtService.Advertise(bid);

            Assert.Equal(dbAdvertisers[0].Id, winner.Id);
            //from notification service, print takes message from queue and calls:
            advertiserRepository.IncrementPrint(winner, wednesday);
            
            
            //from 3rd party controlling this node
            advertiserCache.Update(advertiserRepository.GetAvailableAdvertisers());
            //also periodically called by controller
            advertiserRepository.UpdateHourlyPrints();


            Assert.Equal(dbAdvertisers[0].Id, winner.Id);

            //next advertiser is different
            winner = rtService.Advertise(bid);
            Assert.Equal(dbAdvertisers[1].Id, winner.Id);
        }
    }
}
