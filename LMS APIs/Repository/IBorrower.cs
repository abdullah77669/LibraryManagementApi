using LMS_APIs.Models;

namespace LMS_APIs.Repository
{
    public interface IBorrower
    {
        Task<List<Borrowers>> GetBorrowersAsync(string? search);
        Task<Borrowers?> GetBorrowerByIdAsync(string id);
        Task<ServiceResult<Borrowers>> CreateBorrowerAsync(Borrowers borrower);
        Task<ServiceResult<bool>> UpdateBorrowerAsync(string id, Borrowers borrower);
        Task<ServiceResult<bool>> DeleteBorrowerAsync(string id);
    }
}
