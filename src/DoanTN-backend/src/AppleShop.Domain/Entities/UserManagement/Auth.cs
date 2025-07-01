using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.UserManagement
{
    public class Auth : BaseEntity
    {
        public string? RefreshToken { get; set; }
        public int? UserId { get; set; }
        public int? IsActived { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? Expiration { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}