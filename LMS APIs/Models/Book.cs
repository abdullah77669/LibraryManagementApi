using System.ComponentModel.DataAnnotations;

namespace LMS_APIs.Models
{
    public class Book
    {
        [Key]
        public int BookID { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }

        [Required]
        [MaxLength(50)]
        public required string ISBN { get; set; }

        [Required]
        
        public required int CategoryID { get; set; }

        [Required]
        public required int PublisherID { get; set; }

        [Required]
        public required int AuthorID { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Language { get; set; }

        [Required]
        [MaxLength(2000)]
        public required string Excerpts { get; set; }

        [MaxLength(2048)]
        public string? ImageUrl { get; set; }

        public DateTime PublishDate { get; set; }

    }

}
