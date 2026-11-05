using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMS_APIs.Data;
using LMS_APIs.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LMS_APIs.Repository
{
    public class UserService : IUser
    {
        private readonly ApplicationDBContext _context;
        private readonly IPasswordHasher<Borrowers> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UserService(
            ApplicationDBContext context,
            IPasswordHasher<Borrowers> passwordHasher,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var borrower = await _context.Borrowers
                .SingleOrDefaultAsync(b => b.UserID == request.UserID);

            if (borrower == null || borrower.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(borrower.PasswordHash))
                return null;

            var verification = _passwordHasher.VerifyHashedPassword(
                borrower,
                borrower.PasswordHash,
                request.Password);

            if (verification == PasswordVerificationResult.Failed)
                return null;

            return CreateToken(borrower);
        }

        public async Task<ServiceResult<Borrowers>> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Borrowers.AnyAsync(b => b.UserID == request.UserID))
                return new ServiceResult<Borrowers>(null, 409, "A user with this UserID already exists.");

            var borrower = new Borrowers
            {
                UserID = request.UserID,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Status = "Active",
                Role = "Borrower"
            };

            borrower.PasswordHash = _passwordHasher.HashPassword(borrower, request.Password);
            _context.Borrowers.Add(borrower);
            await _context.SaveChangesAsync();
            return new ServiceResult<Borrowers>(borrower);
        }

        private LoginResponse CreateToken(Borrowers borrower)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = jwtSection["Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured.");
            var issuer = jwtSection["Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
            var audience = jwtSection["Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
            var minutes = int.TryParse(jwtSection["ExpirationMinutes"], out var configuredMinutes)
                ? configuredMinutes
                : 60;
            var expiresAt = DateTime.UtcNow.AddMinutes(minutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, borrower.UserID),
                new Claim(ClaimTypes.NameIdentifier, borrower.UserID),
                new Claim(ClaimTypes.Name, borrower.FullName),
                new Claim(ClaimTypes.Role, borrower.Role)
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new LoginResponse(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt,
                borrower.UserID,
                borrower.Role);
        }
    }
}
