using AppleShop.Application.Requests.AIManagement.AIPrompt;
using AppleShop.Domain.Abstractions.IRepositories.AIManagement;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities.EventManagement;

namespace AppleShop.Application.Handlers.AIManagement.AIPrompt
{
    public class GetAllPromptHandler : IRequestHandler<GetAllPromptRequest, Result<List<Domain.Entities.AIManagement.AIPrompt>>>
    {
        private readonly IAIPromptRepository aIPromptRepository;

        public GetAllPromptHandler(IAIPromptRepository aIPromptRepository)
        {
            this.aIPromptRepository = aIPromptRepository;
        }

        public async Task<Result<List<Domain.Entities.AIManagement.AIPrompt>>> Handle(GetAllPromptRequest request, CancellationToken cancellationToken)
        {
            var prompts = aIPromptRepository.FindAll().ToList();
            return Result<List<Domain.Entities.AIManagement.AIPrompt>>.Ok(prompts);
        }
    }
}