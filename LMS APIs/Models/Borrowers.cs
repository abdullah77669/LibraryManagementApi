using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LMS_APIs.Models
{
    public class Borrowers
    {
        [Key]
        [MaxLength(50)]
        public required string UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FullName { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MaxLength(15)]
        [Phone]
        public required string PhoneNumber { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Status { get; set; }

        [Required]
        [MaxLength(30)]
        public required string Role { get; set; }

        [MaxLength(500)]
        [JsonIgnore]
        public string? PasswordHash { get; set; }
    }
}
