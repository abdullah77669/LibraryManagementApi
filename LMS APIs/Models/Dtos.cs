using System.ComponentModel.DataAnnotations;

namespace LMS_APIs.Models
{
    public static class Statuses
    {
        public const string Available = "Available"; // BookCopies.Status
        public const string Loaned = "Loaned";       // BookCopies.Status
        public const string Active = "Active";       // Loans.Status
        public const string Returned = "Returned";   // Loans.Status
    }

    // Body for POST /api/loans/checkout
    public class CheckoutRequest
    {
        [Required, MaxLength(50)]
        public string BorrowerID { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int BookCopyID { get; set; }

        // Optional. Defaults to 14 days from now.
        public DateTime? DueDate { get; set; }
    }

    // A book together with the names the frontend needs to display it
    public class BookDetails
    {
        public int BookID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Excerpts { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public int AuthorID { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int PublisherID { get; set; }
        public string PublisherName { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }

    // A loan together with borrower, book and branch names
    public class LoanDetails
    {
        public int LoanID { get; set; }
        public string BorrowerID { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public int BookCopyID { get; set; }
        public int BookID { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }
}
