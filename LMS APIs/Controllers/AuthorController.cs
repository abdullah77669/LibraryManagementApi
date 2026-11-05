using LMS_APIs.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YourNamespace.Models;

namespace LMS_APIs.Controllers
{
    [Route("api/authors")]
    [ApiController]
    [Authorize(Roles = "Admin,Librarian")]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthor _authorService;

        public AuthorController(IAuthor authorService)
        {
            _authorService = authorService;
        }

        [HttpGet]
        public IEnumerable<Author> GetAuthors()
        {
            return _authorService.GetAuthors();
        }

        [HttpGet("{id}")]
        public ActionResult<Author> GetAuthorById(int id)
        {
            var author = _authorService.GetAuthorById(id);
            if (author == null) return NotFound("Not Found");
            return author;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Librarian")]
        public ActionResult<Author> PostAuthor(Author author)
        {
            var created = _authorService.PostAuthor(author);
            return CreatedAtAction(nameof(GetAuthorById), new { id = created.AuthorID }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult UpdateAuthor(int id, Author author)
        {
            var updatedAuthor = _authorService.UpdateAuthor(id, author);
            if (updatedAuthor == null) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Librarian")]
        public IActionResult DeleteAuthor(int id)
        {
            var deleted = _authorService.DeleteAuthor(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}