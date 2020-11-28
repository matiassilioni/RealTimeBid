using System;

namespace RealTimeBid.Core
{
    public class Tag
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Tag tag &&
                   Key == tag.Key &&
                   Value == tag.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value);
        }
    }
}
