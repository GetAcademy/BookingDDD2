namespace BookingDDD.Test
{
    using BookingDDD.Core._1_ApplicationServices;
    using BookingDDD.Core._2_DomainServices;
    using BookingDDD.Core._3_Domain_Model;

    public class BookingServiceTests
    {
        [Test]
        public async Task BookAsync_AddsBooking_WhenResourceAcceptsPeriod()
        {
            var repository = new InMemoryBookingRepository();
            var inMemoryResourceRepository = new InMemoryResourceRepository();
            var service = new BookingService(repository, inMemoryResourceRepository);
            var period = TestPeriods.Create(10, 11);

            var result = await service.BookAsync(period);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(repository.Bookings, Has.Count.EqualTo(1));
            Assert.That(repository.Bookings.Single(), Is.SameAs(result.Value));
        }

        [Test]
        public async Task BookAsync_DoesNotAddBooking_WhenResourceRejectsPeriod()
        {
            var repository = new InMemoryBookingRepository();
            var inMemoryResourceRepository = new InMemoryResourceRepository();
            var service = new BookingService(repository, inMemoryResourceRepository);
            var period = TestPeriods.Create(18, 19);

            var result = await service.BookAsync(period);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Booking must be within opening hours."));
            Assert.That(repository.Bookings, Is.Empty);
        }

        [Test]
        public async Task BookAsync_UsesExistingBookingsFromRepository()
        {
            var existingBooking = new Booking(TestPeriods.Create(10, 12));
            var repository = new InMemoryBookingRepository(existingBooking);
            var inMemoryResourceRepository = new InMemoryResourceRepository();
            var service = new BookingService(repository, inMemoryResourceRepository);
            var requestedPeriod = TestPeriods.Create(11, 13);

            var result = await service.BookAsync(requestedPeriod);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Booking overlaps with an existing booking."));
            Assert.That(repository.Bookings, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task CancelAsync_ReturnsFailure_WhenBookingDoesNotExist()
        {
            var repository = new InMemoryBookingRepository();
            var inMemoryResourceRepository = new InMemoryResourceRepository();
            var service = new BookingService(repository, inMemoryResourceRepository);

            var result = await service.CancelAsync(Guid.NewGuid(), new DateTime(2026, 6, 1, 9, 0, 0));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Booking does not exist."));
            Assert.That(repository.UpdateCount, Is.Zero);
        }

        [Test]
        public async Task CancelAsync_UpdatesRepository_WhenBookingCanBeCancelled()
        {
            var booking = new Booking(TestPeriods.Create(10, 11));
            var repository = new InMemoryBookingRepository(booking);
            var inMemoryResourceRepository = new InMemoryResourceRepository();
            var service = new BookingService(repository, inMemoryResourceRepository);

            var result = await service.CancelAsync(booking.Id, new DateTime(2026, 6, 1, 9, 0, 0));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
            Assert.That(repository.UpdateCount, Is.EqualTo(1));
        }

        [Test]
        public async Task CancelAsync_DoesNotUpdateRepository_WhenBookingRejectsCancellation()
        {
            var booking = new Booking(TestPeriods.Create(10, 11));
            var repository = new InMemoryBookingRepository(booking);
            var inMemoryResourceRepository = new InMemoryResourceRepository();
            var service = new BookingService(repository, inMemoryResourceRepository);

            var result = await service.CancelAsync(booking.Id, new DateTime(2026, 6, 1, 10, 0, 0));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage, Is.EqualTo("Cannot cancel booking after it has started."));
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Active));
            Assert.That(repository.UpdateCount, Is.Zero);
        }

        private sealed class InMemoryResourceRepository : IResourceRepository
        {
            private Resource _resource;
            public InMemoryResourceRepository(IEnumerable<Booking>? bookings = null)
            {
                _resource = new Resource(
                    new ResourceId(Guid.Empty), 
                    new OpeningHours(8, 16), 
                    bookings ?? new List<Booking>());
            }

            public Task<Resource> GetAsync(ResourceId ResourceId)
            {
                return Task.FromResult(_resource);
            }
        }

        private sealed class InMemoryBookingRepository : IBookingRepository
        {
            private readonly List<Booking> _bookings;

            public InMemoryBookingRepository(params Booking[] bookings)
            {
                _bookings = bookings.ToList();
            }

            public IReadOnlyList<Booking> Bookings => _bookings;

            public int UpdateCount { get; private set; }

            public Task<List<Booking>> GetAllAsync()
            {
                return Task.FromResult(_bookings.ToList());
            }

            public Task<Booking?> GetAsync(Guid bookingId)
            {
                return Task.FromResult(_bookings.SingleOrDefault(booking => booking.Id == bookingId));
            }

            public Task AddAsync(Booking booking)
            {
                _bookings.Add(booking);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(Booking booking)
            {
                UpdateCount++;
                return Task.CompletedTask;
            }
        }
    }
}
