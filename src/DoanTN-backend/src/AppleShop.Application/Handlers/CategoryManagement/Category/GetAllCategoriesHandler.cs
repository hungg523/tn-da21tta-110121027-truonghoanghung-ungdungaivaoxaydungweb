using AppleShop.Application.Requests.CategoryManagement.Category;
using AppleShop.Application.Requests.DTOs.CategoryManagement.Category;
using AppleShop.Application.Validators.CategoryManagement.Category;
using AppleShop.Domain.Abstractions.IRepositories.CategoryManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.CategoryManagement.Category
{
    public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesRequest, Result<List<CategoryDTO>>>
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly ICacheService cacheService;

        public GetAllCategoriesHandler(ICategoryRepository categoryRepository, ICacheService cacheService)
        {
            this.categoryRepository = categoryRepository;
            this.cacheService = cacheService;
        }

        public async Task<Result<List<CategoryDTO>>> Handle(GetAllCategoriesRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetAllCategoriesValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            //var cacheKey = $"categories_{request.IsActived}";
            //var cachedResult = await cacheService.GetAsync<Result<List<CategoryDTO>>>(cacheKey);
            //if (cachedResult is not null) return cachedResult;

            var query = categoryRepository.FindAll(x => x.CatPid == 0);
            if (request.IsActived is not null) query = query.Where(x => x.IsActived == request.IsActived);

            var categories = query.ToList();
            var categoryDtos = (await Task.WhenAll(categories.Select(async category => new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActived = category.IsActived,
                CreatedAt = category.CreatedDate,
                Categories = await GetSubCategoriesAsync(category.Id.Value)
            }))).ToList();

            var result = Result<List<CategoryDTO>>.Ok(categoryDtos);
            //await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
            return result;
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
                    IsActived = subCategory.IsActived,
                    CreatedAt = subCategory.CreatedDate,
                    Categories = await GetSubCategoriesAsync(subCategory.Id.Value)
                };

                return subCategoryDto;
            }));

            return subCategoriesDto.ToList();
        }
    }
}