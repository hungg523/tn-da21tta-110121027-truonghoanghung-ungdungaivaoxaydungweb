using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.CategoryManagement.Category
{
    public class CreateCategoryRequest : ICommand
    {
        public int? CatPid { get; set; } = 0;
        public string? Name { get; set; }
        public string? Description { get; set; }

        [JsonIgnore]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? IsActived { get; set; } = 0;
    }
}