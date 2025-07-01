using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ProductManagement.Attribute
{
    public class DeleteAttributeRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}