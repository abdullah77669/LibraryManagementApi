using LMS_APIs.Models;
using LMS_APIs.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_APIs.Controllers
{
    [Route("api/loans")]
    [ApiController]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoan _loanService;

        public LoansController(ILoan loanService)
        {
            _loanService = loanService;
        }

        // GET api/loans?status=Active&borrowerId=U001
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanDetails>>> GetLoans([FromQuery] string? status, [FromQuery] string? borrowerId)
        {
            return Ok(await _loanService.GetLoansAsync(status, borrowerId));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<LoanDetails>> GetLoanById(int id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);
            if (loan == null) return NotFound();
            return loan;
        }

        // POST api/loans/checkout   { "borrowerID": "U001", "bookCopyID": 5, "dueDate": "2026-11-20" }
        [HttpPost("checkout")]
        public async Task<ActionResult<LoanDetails>> Checkout(CheckoutRequest request)
        {
            var result = await _loanService.CheckoutAsync(request);
            if (result.Loan == null) return StatusCode(result.ErrorStatus, result.Error);
            return CreatedAtAction(nameof(GetLoanById), new { id = result.Loan.LoanID }, result.Loan);
        }

        // POST api/loans/12/return
        [HttpPost("{id:int}/return")]
        public async Task<ActionResult<LoanDetails>> Return(int id)
        {
            var result = await _loanService.ReturnAsync(id);
            if (result.Loan == null) return StatusCode(result.ErrorStatus, result.Error);
            return Ok(result.Loan);
        }
    }
}