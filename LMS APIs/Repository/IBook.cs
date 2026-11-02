using LMS_APIs.Models;
using YourNamespace.Models;

namespace LMS_APIs.Repository
{
    public interface IBook
    {
        public IEnumerable<Book> GetBooks();
        List<Book> GetAllBookList();
        Book GetBookById(int id);
        public Book PostBook(Book book);
        Book UpdateBook(int id, Book book);
        bool DeleteBook(int id);
    }
}
