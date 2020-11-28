using System;
using System.Threading.Tasks;

namespace RealTimeBid.Core
{
 
    /// <summary>
    /// used to push notifications to a message queue, where many consumer groups will receive a copy of the message and do different task
    /// One queue listener will call AdvertiserRepository.Print()
    /// other queue listener will log, etc...
    /// 
    /// every RTMBidService call will fire and forget this notifications, won't wait for result.
    /// </summary>
    public interface IPrintNotificationService
    {
        Task Push(Bid bid, Advertiser advertiser, DateTime dateTime);
    }

    public class DummyPrintNotificationService : IPrintNotificationService
    {
        public Task Push(Bid bid, Advertiser advertiser, DateTime dateTime)
        {
            return Task.CompletedTask;
        }
    }
}
