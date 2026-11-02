using LMS_APIs.Models;

namespace LMS_APIs.Repository
{
    public interface IBranch
    {
        Task<List<Branch>> GetBranchesAsync();
        Task<Branch?> GetBranchByIdAsync(int id);
        Task<ServiceResult<Branch>> CreateBranchAsync(Branch branch);
        Task<ServiceResult<bool>> UpdateBranchAsync(int id, Branch branch);
        Task<ServiceResult<bool>> DeleteBranchAsync(int id);
    }
}
