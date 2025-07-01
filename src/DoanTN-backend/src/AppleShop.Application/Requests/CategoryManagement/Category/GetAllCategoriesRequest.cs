using AppleShop.Application.Requests.DTOs.CategoryManagement.Category;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.CategoryManagement.Category
{
    public class GetAllCategoriesRequest : IQuery<List<CategoryDTO>>
    {
        public int? IsActived { get; set; }
    }
}