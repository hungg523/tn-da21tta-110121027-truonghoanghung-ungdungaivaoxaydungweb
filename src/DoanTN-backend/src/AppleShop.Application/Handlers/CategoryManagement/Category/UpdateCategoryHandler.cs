using AppleShop.Application.Requests.CategoryManagement.Category;
using AppleShop.Application.Validators.CategoryManagement.Category;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using AutoMapper;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.CategoryManagement.Category
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryRequest, Result<object>>
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IFileService fileUploadService;
        private readonly IMapper mapper;

        public UpdateCategoryHandler(ICategoryRepository categoryRepository, IFileService fileUploadService, IMapper mapper)
        {
            this.categoryRepository = categoryRepository;
            this.fileUploadService = fileUploadService;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var validator = new UpdateCategoryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var category = await categoryRepository.FindByIdAsync(request.Id, true);
            if (category is null) AppleException.ThrowNotFound(typeof(Entities.CategoryManagement.Category));

            if (request.CatPid is not null && request.CatPid > 0)
            {
                var existCategory = await categoryRepository.FindByIdAsync(request.CatPid, true);
                if (existCategory is null) AppleException.ThrowNotFound(typeof(Entities.CategoryManagement.Category));
            }

            using var transaction = await categoryRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                mapper.Map(request, category);
                categoryRepository.Update(category);

                await categoryRepository.SaveChangesAsync(cancellationToken);

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