using BookingDDD.Core._3_Domain_Model;

namespace BookingDDD.Core._2_DomainServices
{
    public interface IResourceRepository
    {
        Task<Resource> GetAsync(ResourceId ResourceId);
    }
}
