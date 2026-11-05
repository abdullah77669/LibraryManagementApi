using LMS_APIs.Models;
using LMS_APIs.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_APIs.Controllers
{
    [Route("api/borrowers")]
    [ApiController]
    public class BorrowersController : ControllerBase
    {
        private readonly IBorrower _borrowerService;

        public BorrowersController(IBorrower borrowerService)
        {
            _borrowerService = borrowerService;
        }

        // GET api/borrowers?search=ali
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Borrowers>>> GetBorrowers([FromQuery] string? search)
        {
            return Ok(await _borrowerService.GetBorrowersAsync(search));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Borrowers>> GetBorrowerById(string id)
        {
            var borrower = await _borrowerService.GetBorrowerByIdAsync(id);
            if (borrower == null) return NotFound();
            return borrower;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult<Borrowers>> PostBorrower(Borrowers borrower)
        {
            var result = await _borrowerService.CreateBorrowerAsync(borrower);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return CreatedAtAction(nameof(GetBorrowerById), new { id = borrower.UserID }, borrower);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> UpdateBorrower(string id, Borrowers borrower)
        {
            var result = await _borrowerService.UpdateBorrowerAsync(id, borrower);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> DeleteBorrower(string id)
        {
            var result = await _borrowerService.DeleteBorrowerAsync(id);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return NoContent();
        }
    }
}