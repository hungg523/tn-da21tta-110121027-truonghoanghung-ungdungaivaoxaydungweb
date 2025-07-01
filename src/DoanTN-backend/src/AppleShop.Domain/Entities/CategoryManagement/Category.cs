using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.CategoryManagement
{
    public class Category : BaseEntity
    {
        public int? CatPid { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? IsActived { get; set; }
    }
}