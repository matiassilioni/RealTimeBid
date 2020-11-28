using System;

namespace RealTimeBid.Core
{
    public class RtmBidService
    {
        private readonly IAdvertisersCache advertisersCache;
        private readonly IPrintNotificationService printNotificationService;
        private readonly IDateTimeService dateTimeService;

        public RtmBidService(
            IAdvertisersCache advertisersCache,
            IPrintNotificationService printNotificationService,
            IDateTimeService dateTimeService
            )
        {
            this.advertisersCache = advertisersCache;
            this.printNotificationService = printNotificationService;
            this.dateTimeService = dateTimeService;
        }

        public Advertiser Advertise(Bid bid)
        {
            var dateTime = this.dateTimeService.GetCurrentDateTime();
            var winner = GetWinner(bid, dateTime);
            if (winner != null) this.printNotificationService.Push(bid, winner, dateTime);
            return winner;
        }
        public Advertiser GetWinner(Bid bid, DateTime dateTime)
        {
            //used a linked list, to easily remove advertisers with rejected tags
            var list = LinkedListAdvertizers.BuildFrom(this.advertisersCache.GetCurrentAdvertisers());
            var prev = list;
            var curr = list;
            //iterate bid tags, remove from the list rejected advertisers and count every passing adv required tag
            for (int i = 0; i < bid.Tags.Count; i++)
            {
                Tag tag = bid.Tags[i];
                //var tagHash = tag.GetHashCode();
                while (curr != null)
                {

                    if (curr.Advertiser.RejectedTagsHashSetsFull.Contains(tag))
                    {
                        //found
                        if (list == curr)
                        {
                            list = curr.Next;
                            prev = curr.Next;
                            curr = curr.Next;
                            continue;
                        }
                        else
                        {
                            prev.Next = curr.Next;
                            curr = curr.Next;
                            continue;
                        }
                    }

                    if (curr.Advertiser.RequiredTagsHashSetsFull.Contains(tag))
                    {
                        //found
                        curr.MissingRequeriments--;
                    }
                    prev = curr;
                    curr = curr.Next;
                    continue;
                }
                prev = list;
                curr = list;
            }

            //iterate results of full passing advertizers picking the best bet win, if any
            curr = list;
            Advertiser winner = null;
            while (curr != null)
            {
                if (curr.MissingRequeriments == 0)
                {
                    //list was previously sorted by CPM so this is the max bet for this requeriments.
                    winner = curr.Advertiser;
                    break;
                }
                curr = curr.Next;
            }
            return winner;
        }
    }
}
