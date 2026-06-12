namespace BookingDDD.Core._3_Domain_Model
{
    public record OpeningHours(int OpensAtHour, int ClosesAtHour)
    {
        public bool Contains(BookingPeriod period)
        {
            return period.Start.Hour >= OpensAtHour &&
                   period.End.Hour <= ClosesAtHour;
        }
    }
}
