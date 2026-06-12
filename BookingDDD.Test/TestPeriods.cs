namespace BookingDDD.Test
{
    using BookingDDD.Core._3_Domain_Model;

    internal static class TestPeriods
    {
        public static BookingPeriod Create(int startHour, int endHour)
        {
            return Create(2026, 6, 1, startHour, endHour);
        }

        public static BookingPeriod Create(int year, int month, int day, int startHour, int endHour)
        {
            var result = BookingPeriod.Create(
                new DateTime(year, month, day, startHour, 0, 0),
                new DateTime(year, month, day, endHour, 0, 0));

            Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
            return result.Value!;
        }
    }
}
