using LMS_APIs.Data;
using LMS_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_APIs.Repository
{
    public class BorrowerService : IBorrower
    {
        private readonly ApplicationDBContext _context;

        public BorrowerService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Borrowers>> GetBorrowersAsync(string? search)
        {
            var query = _context.Borrowers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b => b.FullName.Contains(search)
                                      || b.Email.Contains(search)
                                      || b.UserID.Contains(search));
            }

            return await query.OrderBy(b => b.FullName).ToListAsync();
        }

        public Task<Borrowers?> GetBorrowerByIdAsync(string id)
        {
            return _context.Borrowers.FindAsync(id).AsTask();
        }

        public async Task<ServiceResult<Borrowers>> CreateBorrowerAsync(Borrowers borrower)
        {
            if (await _context.Borrowers.AnyAsync(b => b.UserID == borrower.UserID))
                return new ServiceResult<Borrowers>(null, 409, $"A borrower with UserID '{borrower.UserID}' already exists.");

            _context.Borrowers.Add(borrower);
            await _context.SaveChangesAsync();
            return new ServiceResult<Borrowers>(borrower);
        }

        public async Task<ServiceResult<bool>> UpdateBorrowerAsync(string id, Borrowers borrower)
        {
            var existing = await _context.Borrowers.FindAsync(id);
            if (existing == null)
                return new ServiceResult<bool>(false, 404);

            existing.FullName = borrower.FullName;
            existing.Email = borrower.Email;
            existing.PhoneNumber = borrower.PhoneNumber;
            existing.Address = borrower.Address;
            existing.Status = borrower.Status;
            existing.Role = borrower.Role;
            await _context.SaveChangesAsync();
            return new ServiceResult<bool>(true);
        }

        public async Task<ServiceResult<bool>> DeleteBorrowerAsync(string id)
        {
            var existing = await _context.Borrowers.FindAsync(id);
            if (existing == null)
                return new ServiceResult<bool>(false, 404);

            if (await _context.Loans.AnyAsync(l => l.BorrowerID == id))
                return new ServiceResult<bool>(false, 409, "Cannot delete this borrower because they have loan history.");

            _context.Borrowers.Remove(existing);
            await _context.SaveChangesAsync();
            return new ServiceResult<bool>(true);
        }
    }
}
