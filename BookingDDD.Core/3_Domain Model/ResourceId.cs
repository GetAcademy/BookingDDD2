namespace BookingDDD.Core._3_Domain_Model
{
    public record ResourceId(Guid Value)
    {
        public static ResourceId New() => new(Guid.NewGuid());
    }
}
