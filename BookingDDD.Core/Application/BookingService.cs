using BookingDDD.Core.Abstractions;
using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Application;

public sealed class BookingService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public BookingService(
        IResourceRepository resourceRepository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher eventDispatcher)
    {
        _resourceRepository = resourceRepository;
        _unitOfWork = unitOfWork;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<Result<Booking>> BookAsync(
        ResourceId resourceId,
        BookingPeriod period,
        CancellationToken cancellationToken = default)
    {
        var resource = await _resourceRepository.GetByIdAsync(
            resourceId,
            cancellationToken);

        if (resource is null)
        {
            return Result<Booking>.Fail("Resource does not exist.");
        }

        var result = resource.Book(period);
        if (result.IsFailure)
        {
            return result;
        }

        await SaveCommitAndPublishAsync(resource, cancellationToken);
        return result;
    }

    public async Task<Result<Booking>> CancelAsync(
        ResourceId resourceId,
        BookingId bookingId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var resource = await _resourceRepository.GetByIdAsync(
            resourceId,
            cancellationToken);

        if (resource is null)
        {
            return Result<Booking>.Fail("Resource does not exist.");
        }

        var result = resource.CancelBooking(bookingId, now);
        if (result.IsFailure)
        {
            return result;
        }

        await SaveCommitAndPublishAsync(resource, cancellationToken);
        return result;
    }

    private async Task SaveCommitAndPublishAsync(
        Resource resource,
        CancellationToken cancellationToken)
    {
        try
        {
            await _resourceRepository.SaveAsync(resource, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        var events = resource.DomainEvents.ToArray();
        await _eventDispatcher.PublishAsync(events, cancellationToken);
        resource.ClearDomainEvents();
    }
}
