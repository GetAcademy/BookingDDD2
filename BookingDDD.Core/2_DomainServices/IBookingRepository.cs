using BookingDDD.Core._3_Domain_Model;

namespace BookingDDD.Core._2_DomainServices
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllAsync();
        Task<Booking?> GetAsync(Guid bookingId);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
    }
}
