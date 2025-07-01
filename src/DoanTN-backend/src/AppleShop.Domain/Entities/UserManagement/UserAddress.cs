using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.UserManagement
{
    public class UserAddress : BaseEntity
    {
        public int? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AddressLine { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public bool? Default { get; set; }
    }
}