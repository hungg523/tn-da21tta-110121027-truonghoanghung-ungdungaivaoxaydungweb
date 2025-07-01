using AppleShop.Application.Requests.AIManagement.AIGenerate;
using AppleShop.Application.Requests.DTOs.EventManagement.AIGenerate;
using AppleShop.Share.Shared;
using GenerativeAI;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AppleShop.Application.Handlers.AIManagement.AIGenerate
{
    public class ProductDescriptionGenerateHandler : IRequestHandler<ProductDescriptionGenerateRequest, Result<DescritionDTO>>
    {
        private readonly IConfiguration configuration;

        public ProductDescriptionGenerateHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<Result<DescritionDTO>> Handle(ProductDescriptionGenerateRequest request, CancellationToken cancellationToken)
        {
            var apiKey = configuration["Gemini:ApiKey"];
            var client = new GenerativeModel(model: "gemini-2.0-flash", apiKey: apiKey);

            var response = await client.GenerateContentAsync(request.Prompt, cancellationToken);

            var descriptionDTO = new DescritionDTO { Description = response.Text };

            return Result<DescritionDTO>.Ok(descriptionDTO);
        }
    }
}