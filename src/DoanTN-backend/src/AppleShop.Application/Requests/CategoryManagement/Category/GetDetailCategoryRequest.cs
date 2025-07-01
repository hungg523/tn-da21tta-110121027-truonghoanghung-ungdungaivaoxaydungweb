using AppleShop.Application.Requests.DTOs.CategoryManagement.Category;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.CategoryManagement.Category
{
    public class GetDetailCategoryRequest : IQuery<CategoryDTO>
    {
        public int? Id { get; set; }
    }
}