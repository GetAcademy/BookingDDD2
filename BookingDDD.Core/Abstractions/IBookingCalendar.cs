using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Abstractions;

public interface IBookingCalendar
{
    Task AddAsync(
        BookingId bookingId,
        ResourceId resourceId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        BookingId bookingId,
        CancellationToken cancellationToken = default);
}
