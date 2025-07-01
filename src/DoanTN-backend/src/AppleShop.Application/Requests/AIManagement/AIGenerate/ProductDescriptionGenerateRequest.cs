using AppleShop.Application.Requests.DTOs.EventManagement.AIGenerate;
using AppleShop.Share.Abstractions;

namespace AppleShop.Application.Requests.AIManagement.AIGenerate
{
    public class ProductDescriptionGenerateRequest : ICommand<DescritionDTO>
    {
        public string? Prompt { get; set; }
    }
}