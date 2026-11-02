using LMS_APIs.Data;
using LMS_APIs.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS_APIs.Repository
{
    public class LoanService : ILoan
    {
        private readonly ApplicationDBContext _context;

        public LoanService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<LoanDetails>> GetLoansAsync(string? status, string? borrowerId)
        {
            var query = BuildLoanQuery();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(l => l.Status == status);

            if (!string.IsNullOrWhiteSpace(borrowerId))
                query = query.Where(l => l.BorrowerID == borrowerId);

            return await query.OrderByDescending(l => l.LoanDate).ToListAsync();
        }

        public Task<LoanDetails?> GetLoanByIdAsync(int id)
        {
            return BuildLoanQuery().SingleOrDefaultAsync(l => l.LoanID == id);
        }

        public async Task<LoanResult> CheckoutAsync(CheckoutRequest request)
        {
            if (!await _context.Borrowers.AnyAsync(b => b.UserID == request.BorrowerID))
                return new LoanResult(null, StatusCodes.Status404NotFound, "Borrower was not found.");

            var copy = await _context.BookCopies
                .SingleOrDefaultAsync(c => c.BookCopyID == request.BookCopyID);

            if (copy == null)
                return new LoanResult(null, StatusCodes.Status404NotFound, "Book copy was not found.");

            if (copy.Status != Statuses.Available)
                return new LoanResult(null, StatusCodes.Status409Conflict, "Book copy is not available.");

            var now = DateTime.UtcNow;
            var loan = new Loans
            {
                BorrowerID = request.BorrowerID,
                BookCopyID = request.BookCopyID,
                LoanDate = now,
                DueDate = request.DueDate ?? now.AddDays(14),
                Status = Statuses.Active
            };

            copy.Status = Statuses.Loaned;
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return new LoanResult(await GetLoanByIdAsync(loan.LoanID));
        }

        public async Task<LoanResult> ReturnAsync(int id)
        {
            var loan = await _context.Loans.SingleOrDefaultAsync(l => l.LoanID == id);
            if (loan == null)
                return new LoanResult(null, StatusCodes.Status404NotFound, "Loan was not found.");

            if (loan.Status == Statuses.Returned)
                return new LoanResult(null, StatusCodes.Status409Conflict, "Loan has already been returned.");

            var copy = await _context.BookCopies
                .SingleOrDefaultAsync(c => c.BookCopyID == loan.BookCopyID);

            loan.Status = Statuses.Returned;
            loan.ReturnDate = DateTime.UtcNow;
            if (copy != null)
                copy.Status = Statuses.Available;

            await _context.SaveChangesAsync();
            return new LoanResult(await GetLoanByIdAsync(id));
        }

        private IQueryable<LoanDetails> BuildLoanQuery()
        {
            var today = DateTime.UtcNow;

            return from loan in _context.Loans.AsNoTracking()
                   join borrower in _context.Borrowers on loan.BorrowerID equals borrower.UserID
                   join copy in _context.BookCopies on loan.BookCopyID equals copy.BookCopyID
                   join book in _context.Books on copy.BookID equals book.BookID
                   join branch in _context.Branches on copy.BranchID equals branch.BranchID
                   select new LoanDetails
                   {
                       LoanID = loan.LoanID,
                       BorrowerID = loan.BorrowerID,
                       BorrowerName = borrower.FullName,
                       BookCopyID = loan.BookCopyID,
                       BookID = book.BookID,
                       BookTitle = book.Title,
                       BranchName = branch.Name,
                       LoanDate = loan.LoanDate,
                       DueDate = loan.DueDate,
                       ReturnDate = loan.ReturnDate,
                       Status = loan.Status,
                       IsOverdue = loan.Status == Statuses.Active && loan.DueDate < today
                   };
        }
    }
}
