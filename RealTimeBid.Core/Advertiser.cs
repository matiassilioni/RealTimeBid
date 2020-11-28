using System.Collections.Generic;

namespace RealTimeBid.Core
{
    public class Advertiser
    {
        public string Id { get; set; }
        public decimal CPM { get; set; }
        public decimal Total { get; set; }
        public int RequiredTagsCount { get; set; }
        public HashSet<Tag> RequiredTagsHashSetsFull { get; set; }
        public HashSet<Tag> RejectedTagsHashSetsFull { get; set; }
    }
}
