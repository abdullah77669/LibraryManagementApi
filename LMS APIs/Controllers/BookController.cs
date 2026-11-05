using LMS_APIs.Models;
using LMS_APIs.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourNamespace.Models;

namespace LMS_APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="Admin,Librarian")]
    public class BookController : ControllerBase
    {
        private readonly IBook _bookService;

        public BookController(IBook bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public IEnumerable<Book> GetBooks()
        {
            // Return books from the service
            return _bookService.GetBooks();
        }

        [HttpGet("GetAllBookList")]
        public List<Book> GetAllBookList()
        {
            return _bookService.GetAllBookList();
        }




        [HttpGet("{id}")]
        public ActionResult<Book> GetBookById(int id)
        {
            var book = _bookService.GetBookById(id);
            if (book == null) return NotFound("Not Found");
            return book;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public ActionResult<Book> PostBook(Book book)
        {
            return _bookService.PostBook(book);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult UpdateAuthor(int id, Book book)
        {
            var updatedAuthor = _bookService.UpdateBook(id, book);
            if (updatedAuthor == null) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult DeleteBook(int id)
        {
            var deleted = _bookService.DeleteBook(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
