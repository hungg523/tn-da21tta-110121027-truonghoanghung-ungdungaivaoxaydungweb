using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.EventManagement.Banner
{
    public class DeleteBannerRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
    }
}