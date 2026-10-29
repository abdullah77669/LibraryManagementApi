using System.ComponentModel.DataAnnotations;

namespace LMS_APIs.Models
{
    public class Loans
    {

        [Key]
        public int LoanID { get; set; }

        [Required]
        [MaxLength(50)]
        public required string BorrowerID { get; set; }

        [Required]
        public required int BookCopyID { get; set; }

        [Required]
        public required DateTime LoanDate { get; set; }

        // Null until the book is actually returned
        public DateTime? ReturnDate { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Status { get; set; }

        [Required]
        public required DateTime DueDate { get; set; }


    }
}