namespace BookingDDD.Test
{
    using BookingDDD.Core._3_Domain_Model;

    public class BookingTests
    {
        [Test]
        public void NewBooking_IsActive()
        {
            var booking = new Booking(TestPeriods.Create(10, 11));

            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Active));
            Assert.That(booking.IsActive, Is.True);
        }

        [Test]
        public void Cancel_ReturnsSuccess_WhenNowIsBeforeStart()
        {
            var booking = new Booking(TestPeriods.Create(10, 11));

            var result = booking.Cancel(new DateTime(2026, 6, 1, 9, 0, 0));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.SameAs(booking));
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
            Assert.That(booking.IsActive, Is.False);
        }

        [Test]
        public void Cancel_ReturnsFailure_WhenNowIsAtStart()
        {
            var booking = new Booking(TestPeriods.Create(10, 11));

            var result = booking.Cancel(new DateTime(2026, 6, 1, 10, 0, 0));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Cannot cancel booking after it has started."));
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Active));
        }

        [Test]
        public void Cancel_ReturnsFailure_WhenNowIsAfterStart()
        {
            var booking = new Booking(TestPeriods.Create(10, 11));

            var result = booking.Cancel(new DateTime(2026, 6, 1, 10, 30, 0));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Cannot cancel booking after it has started."));
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Active));
        }
    }
}
