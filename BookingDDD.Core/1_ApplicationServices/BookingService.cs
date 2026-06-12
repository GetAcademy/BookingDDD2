using BookingDDD.Core._2_DomainServices;
using BookingDDD.Core._3_Domain_Model;

namespace BookingDDD.Core._1_ApplicationServices
{
    public class BookingService
    {
        private static readonly ResourceId ResourceId = new(Guid.Empty);

        private readonly IBookingRepository _bookingRepository;
        //private readonly OpeningHours _openingHours = new(8, 16);
        private IResourceRepository _resourceRepository;

        public BookingService(IBookingRepository bookingRepository, IResourceRepository resourceRepository)
        {
            _resourceRepository = resourceRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<Booking>> BookAsync(BookingPeriod period)
        {
            var resource = await _resourceRepository.GetAsync(ResourceId);
            var bookingResult = resource.Book(period);
            if (!bookingResult.IsSuccess)
            {
                return bookingResult;
            }

            //await _resourceRepository.UpdateAsync(resource);
            await _bookingRepository.AddAsync(bookingResult.Value!);
            return bookingResult;
        }

        public async Task<Result<Booking>> CancelAsync(Guid bookingId, DateTime now)
        {
            var booking = await _bookingRepository.GetAsync(bookingId);
            if (booking == null)
            {
                return Result<Booking>.Fail("Booking does not exist.");
            }

            var cancelResult = booking.Cancel(now);
            if (!cancelResult.IsSuccess)
            {
                return cancelResult;
            }

            await _bookingRepository.UpdateAsync(booking);
            return Result<Booking>.Success(booking);
        }
    }
}
