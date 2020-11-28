using System.Collections.Generic;

namespace RealTimeBid.Core
{
    public class LinkedListAdvertizers
    {
        public Advertiser Advertiser { get; set; }
        public int MissingRequeriments { get; set; }
        public LinkedListAdvertizers Next { get; set; }

        public static LinkedListAdvertizers BuildFrom(List<Advertiser> advertisers)
        {
            LinkedListAdvertizers root = null;
            LinkedListAdvertizers current = null;
            foreach (var advertiser in advertisers)
            {
                if (current != null)
                {
                    current.Next = new LinkedListAdvertizers { Advertiser = advertiser, MissingRequeriments = advertiser.RequiredTagsCount };
                    current = current.Next;
                }
                else
                {
                    current = new LinkedListAdvertizers { Advertiser = advertiser, MissingRequeriments = advertiser.RequiredTagsCount };
                    root = current;
                }
            }
            return root;
        }
    }
}
