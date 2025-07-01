using AppleShop.Application.Requests.CategoryManagement.Category;
using AppleShop.Application.Requests.DTOs.CategoryManagement.Category;
using AppleShop.Application.Validators.CategoryManagement.Category;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.CategoryManagement.Category
{
    public class GetDetailCategoryHandler : IRequestHandler<GetDetailCategoryRequest, Result<CategoryDTO>>
    {
        private readonly ICategoryRepository categoryRepository;

        public GetDetailCategoryHandler(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async Task<Result<CategoryDTO>> Handle(GetDetailCategoryRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetDetailCategoryValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var category = await categoryRepository.FindSingleAsync(x => x.Id == request.Id && x.IsActived > 0);
            if (category is null) AppleException.ThrowNotFound(typeof(Entities.CategoryManagement.Category));

            var categoryDto = new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Categories = await GetSubCategoriesAsync(category.Id.Value)
            };

            return Result<CategoryDTO>.Ok(categoryDto);
        }

        private async Task<List<CategoryDTO>> GetSubCategoriesAsync(int pId)
        {
            List<Entities.CategoryManagement.Category> subCategories = categoryRepository.FindAll(x => x.CatPid.Value == pId && x.IsActived > 0).ToList();

            var subCategoriesDto = await Task.WhenAll(subCategories.Select(async subCategory =>
            {
                CategoryDTO subCategoryDto = new CategoryDTO
                {
                    Id = subCategory.Id,
                    Name = subCategory.Name,
                    Description = subCategory.Description,
                    Categories = await GetSubCategoriesAsync(subCategory.Id.Value)
                };

                return subCategoryDto;
            }));

            return subCategoriesDto.ToList();
        }
    }
}