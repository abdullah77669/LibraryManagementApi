using LMS_APIs.Models;

namespace LMS_APIs.Repository
{
    // Loan is set on success; otherwise ErrorStatus (400/404/409) and Error explain what went wrong
    public record LoanResult(LoanDetails? Loan, int ErrorStatus = 0, string? Error = null);

    public interface ILoan
    {
        Task<List<LoanDetails>> GetLoansAsync(string? status, string? borrowerId);
        Task<LoanDetails?> GetLoanByIdAsync(int id);
        Task<LoanResult> CheckoutAsync(CheckoutRequest request);
        Task<LoanResult> ReturnAsync(int id);
    }
}