using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application;

public sealed class AuditBookingCancelledHandler(IAuditLog auditLog)
    : IDomainEventHandler<BookingCancelled>
{
    public Task HandleAsync(
        BookingCancelled domainEvent,
        CancellationToken cancellationToken = default) =>
        auditLog.RecordAsync(
            nameof(BookingCancelled),
            domainEvent.BookingId,
            domainEvent.ResourceId,
            DateTime.UtcNow,
            cancellationToken);
}

public sealed class RemoveBookingFromCalendarHandler(IBookingCalendar calendar)
    : IDomainEventHandler<BookingCancelled>
{
    public Task HandleAsync(
        BookingCancelled domainEvent,
        CancellationToken cancellationToken = default) =>
        calendar.RemoveAsync(
            domainEvent.BookingId,
            cancellationToken);
}

public sealed class SendBookingCancellationHandler(
    IBookingNotification notification)
    : IDomainEventHandler<BookingCancelled>
{
    public Task HandleAsync(
        BookingCancelled domainEvent,
        CancellationToken cancellationToken = default) =>
        notification.SendCancelledAsync(
            domainEvent.BookingId,
            domainEvent.ResourceId,
            cancellationToken);
}
