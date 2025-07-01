using AppleShop.Domain.Abstractions.Common;

namespace AppleShop.Domain.Entities.WishListManagement;

public class WishList : BaseEntity
{
    public int? UserId { get; set; }
    public int? VariantId { get; set; }
    public DateTime? AddedDate { get; set; }
    public bool? IsActived { get; set; }
}