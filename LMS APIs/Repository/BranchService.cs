using LMS_APIs.Data;
using LMS_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_APIs.Repository
{
    public class BranchService : IBranch
    {
        private readonly ApplicationDBContext _context;

        public BranchService(ApplicationDBContext context)
        {
            _context = context;
        }

        public Task<List<Branch>> GetBranchesAsync()
        {
            return _context.Branches.AsNoTracking().OrderBy(b => b.Name).ToListAsync();
        }

        public Task<Branch?> GetBranchByIdAsync(int id)
        {
            return _context.Branches.FindAsync(id).AsTask();
        }

        public async Task<ServiceResult<Branch>> CreateBranchAsync(Branch branch)
        {
            branch.BranchID = 0;
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return new ServiceResult<Branch>(branch);
        }

        public async Task<ServiceResult<bool>> UpdateBranchAsync(int id, Branch branch)
        {
            var existing = await _context.Branches.FindAsync(id);
            if (existing == null)
                return new ServiceResult<bool>(false, 404);

            existing.Name = branch.Name;
            existing.Address = branch.Address;
            existing.ContactNumber = branch.ContactNumber;
            await _context.SaveChangesAsync();
            return new ServiceResult<bool>(true);
        }

        public async Task<ServiceResult<bool>> DeleteBranchAsync(int id)
        {
            var existing = await _context.Branches.FindAsync(id);
            if (existing == null)
                return new ServiceResult<bool>(false, 404);
            if (await _context.BookCopies.AnyAsync(c => c.BranchID == id))
                return new ServiceResult<bool>(false, 409, "Cannot delete this branch because it still has book copies.");

            _context.Branches.Remove(existing);
            await _context.SaveChangesAsync();
            return new ServiceResult<bool>(true);
        }
    }
}
