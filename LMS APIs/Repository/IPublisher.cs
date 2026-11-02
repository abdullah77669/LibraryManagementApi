using LMS_APIs.Models;

namespace LMS_APIs.Repository
{
    public interface IPublisher
    {
        Task<List<Publishers>> GetPublishersAsync();
        Task<Publishers?> GetPublisherByIdAsync(int id);
        Task<ServiceResult<Publishers>> CreatePublisherAsync(Publishers publisher);
        Task<ServiceResult<bool>> UpdatePublisherAsync(int id, Publishers publisher);
        Task<ServiceResult<bool>> DeletePublisherAsync(int id);
    }
}
