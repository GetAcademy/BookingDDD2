namespace BookingDDD.Test
{
    using BookingDDD.Core._3_Domain_Model;

    public class BookingPeriodTests
    {
        [Test]
        public void Create_ReturnsFailure_WhenStartIsNotBeforeEnd()
        {
            var start = new DateTime(2026, 6, 1, 10, 0, 0);
            var end = new DateTime(2026, 6, 1, 10, 0, 0);

            var result = BookingPeriod.Create(start, end);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Start must be before end."));
        }

        [Test]
        public void Create_ReturnsFailure_WhenPeriodDoesNotUseWholeHours()
        {
            var start = new DateTime(2026, 6, 1, 10, 30, 0);
            var end = new DateTime(2026, 6, 1, 11, 0, 0);

            var result = BookingPeriod.Create(start, end);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Only whole hours can be booked."));
        }

        [Test]
        public void Create_AllowsPeriodsOutsideOpeningHours()
        {
            var start = new DateTime(2026, 6, 1, 18, 0, 0);
            var end = new DateTime(2026, 6, 1, 19, 0, 0);

            var result = BookingPeriod.Create(start, end);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Start, Is.EqualTo(start));
            Assert.That(result.Value.End, Is.EqualTo(end));
        }

        [Test]
        public void Equality_UsesStartAndEnd()
        {
            var start = new DateTime(2026, 6, 1, 10, 0, 0);
            var end = new DateTime(2026, 6, 1, 11, 0, 0);

            var first = BookingPeriod.Create(start, end).Value!;
            var second = BookingPeriod.Create(start, end).Value!;

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first == second, Is.True);
        }

        [Test]
        public void IsOverlapping_ReturnsTrue_WhenPeriodsOverlap()
        {
            var first = TestPeriods.Create(10, 12);
            var second = TestPeriods.Create(11, 13);

            var overlaps = first.Overlaps(second);

            Assert.That(overlaps, Is.True);
        }

        [Test]
        public void IsOverlapping_ReturnsFalse_WhenPeriodsAreAdjacent()
        {
            var first = TestPeriods.Create(10, 11);
            var second = TestPeriods.Create(11, 12);

            var overlaps = first.Overlaps(second);

            Assert.That(overlaps, Is.False);
        }
    }
}
