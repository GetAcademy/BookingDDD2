using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application;

public sealed class AuditBookingCreatedHandler(IAuditLog auditLog)
    : IDomainEventHandler<BookingCreated>
{
    public Task HandleAsync(
        BookingCreated domainEvent,
        CancellationToken cancellationToken = default) =>
        auditLog.RecordAsync(
            nameof(BookingCreated),
            domainEvent.BookingId,
            domainEvent.ResourceId,
            DateTime.UtcNow,
            cancellationToken);
}

public sealed class AddBookingToCalendarHandler(IBookingCalendar calendar)
    : IDomainEventHandler<BookingCreated>
{
    public Task HandleAsync(
        BookingCreated domainEvent,
        CancellationToken cancellationToken = default) =>
        calendar.AddAsync(
            domainEvent.BookingId,
            domainEvent.ResourceId,
            domainEvent.Start,
            domainEvent.End,
            cancellationToken);
}

public sealed class SendBookingConfirmationHandler(
    IBookingNotification notification)
    : IDomainEventHandler<BookingCreated>
{
    public Task HandleAsync(
        BookingCreated domainEvent,
        CancellationToken cancellationToken = default) =>
        notification.SendCreatedAsync(
            domainEvent.BookingId,
            domainEvent.ResourceId,
            domainEvent.Start,
            domainEvent.End,
            cancellationToken);
}
