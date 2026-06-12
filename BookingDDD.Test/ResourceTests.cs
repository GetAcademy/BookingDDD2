namespace BookingDDD.Test
{
    using BookingDDD.Core._3_Domain_Model;

    public class ResourceTests
    {
        private static readonly OpeningHours OpeningHours = new(8, 16);

        [Test]
        public void Book_ReturnsBooking_WhenPeriodIsAvailableAndInsideOpeningHours()
        {
            var resource = CreateResource();
            var period = TestPeriods.Create(10, 11);

            var result = resource.Book(period);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value!.Period, Is.EqualTo(period));
            Assert.That(result.Value.Status, Is.EqualTo(BookingStatus.Active));
        }

        [Test]
        public void Book_ReturnsFailure_WhenPeriodIsOutsideOpeningHours()
        {
            var resource = CreateResource();
            var period = TestPeriods.Create(18, 19);

            var result = resource.Book(period);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Booking must be within opening hours."));
        }

        [Test]
        public void Cannot_book_resource_when_resource_is_uavailable_in_period()
        {
            var existing = new Booking(TestPeriods.Create(10, 12));
            var resource = CreateResource(existing);
            var requestedPeriod = TestPeriods.Create(11, 13);

            var result = resource.Book(requestedPeriod);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Booking overlaps with an existing booking."));
        }

        [Test]
        public void Book_ReturnsBooking_WhenOverlappingBookingIsCancelled()
        {
            var cancelledBooking = new Booking(
                Guid.NewGuid(),
                TestPeriods.Create(10, 12),
                BookingStatus.Cancelled);
            var resource = CreateResource(cancelledBooking);
            var requestedPeriod = TestPeriods.Create(11, 13);

            var result = resource.Book(requestedPeriod);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Book_ReturnsBooking_WhenPeriodIsAdjacentToExistingBooking()
        {
            var existing = new Booking(TestPeriods.Create(10, 11));
            var resource = CreateResource(existing);
            var requestedPeriod = TestPeriods.Create(11, 12);

            var result = resource.Book(requestedPeriod);

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public void Book_TracksAcceptedBookings_WhenCalledMultipleTimes()
        {
            var resource = CreateResource();
            var firstResult = resource.Book(TestPeriods.Create(10, 12));

            var secondResult = resource.Book(TestPeriods.Create(11, 13));

            Assert.That(firstResult.IsSuccess, Is.True);
            Assert.That(secondResult.IsSuccess, Is.False);
            Assert.That(secondResult.ErrorMessage, Is.EqualTo("Booking overlaps with an existing booking."));
        }

        private static Resource CreateResource(params Booking[] bookings)
        {
            return new Resource(ResourceId.New(), OpeningHours, bookings);
        }
    }
}
