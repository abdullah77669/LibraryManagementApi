using System.ComponentModel.DataAnnotations;

namespace LMS_APIs.Models
{
    public class LoginRequest
    {
        [Required, MaxLength(50)]
        public string UserID { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required, MaxLength(50)]
        public string UserID { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }

    public record LoginResponse(string Token, DateTime ExpiresAt, string UserID, string Role);
}
