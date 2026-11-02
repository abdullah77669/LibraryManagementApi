using LMS_APIs.Data;
using LMS_APIs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_APIs.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CategoriesController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookCategory>>> GetCategories()
        {
            return Ok(await _context.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookCategory>> GetCategoryById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return category;
        }

        [HttpPost]
        public async Task<ActionResult<BookCategory>> PostCategory(BookCategory category)
        {
            category.BookCategoryID = 0;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.BookCategoryID }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, BookCategory category)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return NotFound();

            existing.CategoryName = category.CategoryName;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var existing = await _context.Categories.FindAsync(id);
            if (existing == null) return NotFound();

            if (await _context.Books.AnyAsync(b => b.CategoryID == id))
                return Conflict("Cannot delete this category because books still use it.");

            _context.Categories.Remove(existing);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}