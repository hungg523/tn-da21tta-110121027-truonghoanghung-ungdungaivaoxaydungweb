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
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryRequest, Result<object>>
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IFileService fileUploadService;
        private readonly IMapper mapper;

        public CreateCategoryHandler(ICategoryRepository categoryRepository, IFileService fileUploadService, IMapper mapper)
        {
            this.categoryRepository = categoryRepository;
            this.fileUploadService = fileUploadService;
            this.mapper = mapper;
        }

        public async Task<Result<object>> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var validator = new CreateCategoryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            if (request.CatPid is not null && request.CatPid > 0)
            {
                var existCategory = await categoryRepository.FindByIdAsync(request.CatPid, true);
                if (existCategory is null) AppleException.ThrowNotFound(typeof(Entities.CategoryManagement.Category));
            }

            var category = mapper.Map<Entities.CategoryManagement.Category>(request);

            using var transaction = await categoryRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                categoryRepository.Create(category);
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