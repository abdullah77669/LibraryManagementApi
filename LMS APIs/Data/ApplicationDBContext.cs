using LMS_APIs.Models;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Models;

namespace LMS_APIs.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> Categories { get; set; }
        public DbSet<Publishers> Publishers { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<BookCopies> BookCopies { get; set; }
        public DbSet<Borrowers> Borrowers { get; set; }
        public DbSet<Loans> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // "Publishers" (plural type name) is not picked up by EF's key convention for PublisherID
            modelBuilder.Entity<Publishers>().HasKey(p => p.PublisherID);

            // Books -> Author / Category / Publisher (no navigation properties, so the JSON shape stays the same)
            modelBuilder.Entity<Book>(e =>
            {
                e.HasIndex(b => b.ISBN).IsUnique();
                e.HasOne<Author>().WithMany().HasForeignKey(b => b.AuthorID).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BookCategory>().WithMany().HasForeignKey(b => b.CategoryID).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<Publishers>().WithMany().HasForeignKey(b => b.PublisherID).OnDelete(DeleteBehavior.Restrict);
            });

            // Physical copies: which book, in which branch
            modelBuilder.Entity<BookCopies>(e =>
            {
                e.HasOne<Book>().WithMany().HasForeignKey(c => c.BookID).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<Branch>().WithMany().HasForeignKey(c => c.BranchID).OnDelete(DeleteBehavior.Restrict);
            });

            // Loans: who borrowed which copy
            modelBuilder.Entity<Loans>(e =>
            {
                e.HasOne<Borrowers>().WithMany().HasForeignKey(l => l.BorrowerID).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BookCopies>().WithMany().HasForeignKey(l => l.BookCopyID).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}