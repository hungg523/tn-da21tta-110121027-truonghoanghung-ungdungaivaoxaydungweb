using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.DTOs.CategoryManagement.Category
{
    public class CategoryDTO
    {
        public int? Id { get; set; }
        public string? Name { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? IsActived { get; set; }
        public List<CategoryDTO>? Categories { get; set; } = new List<CategoryDTO>();
    }
}