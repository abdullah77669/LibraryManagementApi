using LMS_APIs.Models;

namespace LMS_APIs.Repository
{
    public interface IBookCopy
    {
        Task<List<BookCopies>> GetCopiesAsync(int? bookId, int? branchId);
        Task<BookCopies?> GetCopyByIdAsync(int id);
        Task<ServiceResult<BookCopies>> CreateCopyAsync(BookCopies copy);
        Task<ServiceResult<bool>> UpdateCopyAsync(int id, BookCopies copy);
        Task<ServiceResult<bool>> DeleteCopyAsync(int id);
    }
}
