using LMS_APIs.Data;
using LMS_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_APIs.Repository
{
    public class BookCopyService : IBookCopy
    {
        private readonly ApplicationDBContext _context;

        public BookCopyService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<BookCopies>> GetCopiesAsync(int? bookId, int? branchId)
        {
            var query = _context.BookCopies.AsNoTracking();
            if (bookId.HasValue) query = query.Where(c => c.BookID == bookId.Value);
            if (branchId.HasValue) query = query.Where(c => c.BranchID == branchId.Value);
            return await query.OrderBy(c => c.BookID).ThenBy(c => c.CopyNumber).ToListAsync();
        }

        public Task<BookCopies?> GetCopyByIdAsync(int id)
        {
            return _context.BookCopies.FindAsync(id).AsTask();
        }

        public async Task<ServiceResult<BookCopies>> CreateCopyAsync(BookCopies copy)
        {
            if (!await _context.Books.AnyAsync(b => b.BookID == copy.BookID))
                return new ServiceResult<BookCopies>(null, 400, $"Book {copy.BookID} does not exist.");
            if (!await _context.Branches.AnyAsync(b => b.BranchID == copy.BranchID))
                return new ServiceResult<BookCopies>(null, 400, $"Branch {copy.BranchID} does not exist.");
            if (await _context.BookCopies.AnyAsync(c => c.BookID == copy.BookID
                                                     && c.BranchID == copy.BranchID
                                                     && c.CopyNumber == copy.CopyNumber))
                return new ServiceResult<BookCopies>(null, 409, "This book already has a copy with that number in that branch.");

            copy.BookCopyID = 0;
            if (string.IsNullOrWhiteSpace(copy.Status)) copy.Status = Statuses.Available;
            copy.CreatedOn = DateTime.UtcNow;
            _context.BookCopies.Add(copy);
            await _context.SaveChangesAsync();
            return new ServiceResult<BookCopies>(copy);
        }

        public async Task<ServiceResult<bool>> UpdateCopyAsync(int id, BookCopies copy)
        {
            var existing = await _context.BookCopies.FindAsync(id);
            if (existing == null)
                return new ServiceResult<bool>(false, 404);
            if (!await _context.Branches.AnyAsync(b => b.BranchID == copy.BranchID))
                return new ServiceResult<bool>(false, 400, $"Branch {copy.BranchID} does not exist.");

            existing.BranchID = copy.BranchID;
            existing.CopyNumber = copy.CopyNumber;
            existing.Status = copy.Status;
            existing.LastUpdatedBy = copy.LastUpdatedBy;
            existing.LastUpdatedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new ServiceResult<bool>(true);
        }

        public async Task<ServiceResult<bool>> DeleteCopyAsync(int id)
        {
            var existing = await _context.BookCopies.FindAsync(id);
            if (existing == null)
                return new ServiceResult<bool>(false, 404);
            if (await _context.Loans.AnyAsync(l => l.BookCopyID == id))
                return new ServiceResult<bool>(false, 409, "Cannot delete this copy because it has loan history.");

            _context.BookCopies.Remove(existing);
            await _context.SaveChangesAsync();
            return new ServiceResult<bool>(true);
        }
    }
}
