using LMS_APIs.Data;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;

namespace LMS_APIs.Repository
{
    public class AuthorService : IAuthor
    {
        private readonly ApplicationDBContext _context;

        public AuthorService(ApplicationDBContext dbContext)
        {
            this._context = dbContext;
        }
        public IEnumerable<Author> GetAuthors()
        {
            return _context.Authors.ToList();
        }

        public Author GetAuthorById(int id)
        {
            return _context.Authors.Find(id);
        }

        public Author PostAuthor(Author author)
        {
            _context.Authors.Add(author);
            _context.SaveChanges();
            return author;
        }

        public Author UpdateAuthor(int id, Author author)
        {
            var existingAuthor = _context.Authors.Find(id);
            if (existingAuthor == null) return null;

            existingAuthor.Name = author.Name;
            existingAuthor.DateOfBirth = author.DateOfBirth;
            existingAuthor.Biography = author.Biography;

            _context.SaveChanges();
            return existingAuthor;
        }

        public bool DeleteAuthor(int id)
        {
            var author = _context.Authors.Find(id);
            if (author == null) return false;

            _context.Authors.Remove(author);
            _context.SaveChanges();
            return true;
        }



    }
}