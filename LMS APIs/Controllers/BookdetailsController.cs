using LMS_APIs.Data;
using LMS_APIs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_APIs.Controllers
{
    // Read-only view of books with author, category and publisher names plus copy counts.
    // Create/update/delete still go through /api/Book.
    [Route("api/book-details")]
    [ApiController]
    public class BookDetailsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public BookDetailsController(ApplicationDBContext context)
        {
            _context = context;
        }

        private IQueryable<BookDetails> BuildQuery()
        {
            return from b in _context.Books
                   join a in _context.Authors on b.AuthorID equals a.AuthorID
                   join c in _context.Categories on b.CategoryID equals c.BookCategoryID
                   join p in _context.Publishers on b.PublisherID equals p.PublisherID
                   select new BookDetails
                   {
                       BookID = b.BookID,
                       Title = b.Title,
                       ISBN = b.ISBN,
                       Language = b.Language,
                       Excerpts = b.Excerpts,
                       PublishDate = b.PublishDate,
                       AuthorID = a.AuthorID,
                       AuthorName = a.Name,
                       CategoryID = c.BookCategoryID,
                       CategoryName = c.CategoryName,
                       PublisherID = p.PublisherID,
                       PublisherName = p.Name,
                       TotalCopies = _context.BookCopies.Count(x => x.BookID == b.BookID),
                       AvailableCopies = _context.BookCopies.Count(x => x.BookID == b.BookID
                                            && (x.Status == null || x.Status == Statuses.Available))
                   };
        }

        // GET api/book-details?search=harry
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDetails>>> GetBookDetails([FromQuery] string? search)
        {
            var query = BuildQuery();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Title.Contains(search)
                                      || x.ISBN.Contains(search)
                                      || x.AuthorName.Contains(search));
            }
            return Ok(await query.AsNoTracking().OrderBy(x => x.Title).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDetails>> GetBookDetailsById(int id)
        {
            var book = await BuildQuery().AsNoTracking().FirstOrDefaultAsync(x => x.BookID == id);
            if (book == null) return NotFound();
            return book;
        }
    }
}