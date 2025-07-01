using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.UserManagement
{
    public class User : BaseEntity
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImageUrl { get; set; }
        public int? Role { get; set; }
        public int? IsActived { get; set; }
        public string? OTP { get; set; }
        public int? OTPAttempt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? OTPExpiration { get; set; }
    }
}