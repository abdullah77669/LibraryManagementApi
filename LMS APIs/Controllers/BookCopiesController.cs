using LMS_APIs.Models;
using LMS_APIs.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_APIs.Controllers
{
    [Route("api/bookcopies")]
    [ApiController]
    public class BookCopiesController : ControllerBase
    {
        private readonly IBookCopy _bookCopyService;

        public BookCopiesController(IBookCopy bookCopyService)
        {
            _bookCopyService = bookCopyService;
        }

        // GET api/bookcopies?bookId=1&branchId=2
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookCopies>>> GetCopies([FromQuery] int? bookId, [FromQuery] int? branchId)
        {
            return Ok(await _bookCopyService.GetCopiesAsync(bookId, branchId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookCopies>> GetCopyById(int id)
        {
            var copy = await _bookCopyService.GetCopyByIdAsync(id);
            if (copy == null) return NotFound();
            return copy;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult<BookCopies>> PostCopy(BookCopies copy)
        {
            var result = await _bookCopyService.CreateCopyAsync(copy);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return CreatedAtAction(nameof(GetCopyById), new { id = copy.BookCopyID }, copy);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> UpdateCopy(int id, BookCopies copy)
        {
            var result = await _bookCopyService.UpdateCopyAsync(id, copy);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<IActionResult> DeleteCopy(int id)
        {
            var result = await _bookCopyService.DeleteCopyAsync(id);
            if (!result.Succeeded) return StatusCode(result.ErrorStatus, result.Error);
            return NoContent();
        }
    }
}