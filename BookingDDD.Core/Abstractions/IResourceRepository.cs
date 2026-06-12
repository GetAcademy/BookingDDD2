using BookingDDD.Core.Domain;

namespace BookingDDD.Core.Abstractions;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(
        ResourceId resourceId,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        Resource resource,
        CancellationToken cancellationToken = default);
}
