using LMS_APIs.Data;
using LMS_APIs.Models;
using YourNamespace.Models;

namespace LMS_APIs.Repository
{
    public class BookService : IBook
    {
        private readonly ApplicationDBContext _context;

        public BookService(ApplicationDBContext dbContext)
        {
            this._context = dbContext;
        }
        public IEnumerable<Book> GetBooks()
        {
            return _context.Books.ToList();
        }

        public Book GetBookById(int id)
        {
            return _context.Books.Find(id);
        }

        public Book PostBook(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
            return book;
        }

        public Book UpdateBook(int id, Book book)
        {
            var existingBook = _context.Books.Find(id);
            if (existingBook == null) return null;

            existingBook.Title = book.Title;
            existingBook.ISBN = book.ISBN;
            existingBook.PublisherID = book.PublisherID;
            existingBook.CategoryID = book.CategoryID;
            existingBook.AuthorID = book.AuthorID;
            existingBook.Language = book.Language;
            existingBook.Excerpts = book.Excerpts;
            existingBook.PublishDate = book.PublishDate;
            _context.SaveChanges();
            return existingBook;
        }

        public bool DeleteBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            _context.SaveChanges();
            return true;
        }


    }
}