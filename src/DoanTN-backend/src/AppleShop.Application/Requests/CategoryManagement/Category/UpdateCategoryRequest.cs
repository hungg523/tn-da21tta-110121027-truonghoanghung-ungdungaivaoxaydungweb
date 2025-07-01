using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.CategoryManagement.Category
{
    public class UpdateCategoryRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public int? CatPid { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? IsActived { get; set; }
    }
}