using AppleShop.Application.Requests.AIManagement.AIPrompt;
using AppleShop.Application.Validators.AIManagement.AIPrompt;
using AppleShop.Domain.Abstractions.IRepositories.AIManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.AIManagement.AIPrompt
{
    public class UpdatePromptHandler : IRequestHandler<UpdatePromptRequest, Result<object>>
    {
        private readonly IAIPromptRepository aIPromptRepository;

        public UpdatePromptHandler(IAIPromptRepository aIPromptRepository)
        {
            this.aIPromptRepository = aIPromptRepository;
        }

        public async Task<Result<object>> Handle(UpdatePromptRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdatePromptValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var prompt = await aIPromptRepository.FindByIdAsync(request.Id, true);
            if (prompt is null) AppleException.ThrowNotFound(typeof(Domain.Entities.AIManagement.AIPrompt));

            using var transaction = await aIPromptRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                prompt.Name = request.Name ?? prompt.Name;
                prompt.Content = request.Content ?? prompt.Content;

                aIPromptRepository.Update(prompt);
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