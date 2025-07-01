using AppleShop.Application.Requests.AIManagement.AIPrompt;
using AppleShop.Application.Validators.AIManagement.AIPrompt;
using AppleShop.Domain.Abstractions.IRepositories.AIManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities.AIManagement;

namespace AppleShop.Application.Handlers.AIManagement.AIPrompt
{
    public class CreatePromptHandler : IRequestHandler<CreatePromptRequest, Result<object>>
    {
        private readonly IAIPromptRepository aIPromptRepository;

        public CreatePromptHandler(IAIPromptRepository aIPromptRepository)
        {
            this.aIPromptRepository = aIPromptRepository;
        }

        public async Task<Result<object>> Handle(CreatePromptRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreatePromptValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            using var transaction = await aIPromptRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                var prompt = new Entities.AIPrompt
                {
                    Name = request.Name,
                    Content = request.Content,
                    CreatedAt = DateTime.Now,
                };

                aIPromptRepository.Create(prompt);
                await aIPromptRepository.SaveChangesAsync(cancellationToken);
                transaction.Commit();
                return Result<object>.Ok();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}