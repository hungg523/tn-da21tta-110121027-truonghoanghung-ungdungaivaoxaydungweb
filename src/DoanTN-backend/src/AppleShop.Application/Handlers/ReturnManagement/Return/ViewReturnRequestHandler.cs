using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Application.Requests.DTOs.ReturnManagement.Return;
using AppleShop.Application.Requests.ReturnManagement.Return;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReturnManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;
using static MassTransit.ValidationResultExtensions;

namespace AppleShop.Application.Handlers.ReturnManagement.Return
{
    public class ViewReturnRequestHandler : IRequestHandler<ViewReturnHistoryRequest, Result<ReturnHistoryResponseDTO>>
    {
        private readonly IReturnRepository returnRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IFileService fileService;
        private readonly ICacheService cacheService;

        public ViewReturnRequestHandler(IReturnRepository returnRepository,
                                        IOrderRepository orderRepository,
                                        IOrderItemRepository orderItemRepository,
                                        IProductAttributeRepository productAttributeRepository,
                                        IProductVariantRepository productVariantRepository,
                                        IAttributeValueRepository attributeValueRepository,
                                        IProductRepository productRepository,
                                        IProductImageRepository productImageRepository,
                                        IFileService fileService,
                                        ICacheService cacheService)
        {
            this.returnRepository = returnRepository;
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.productVariantRepository = productVariantRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.fileService = fileService;
            this.cacheService = cacheService;
        }

        public async Task<Result<ReturnHistoryResponseDTO>> Handle(ViewReturnHistoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var cacheKey = $"return_history_{request.UserId}_{request.Skip}_{request.Take}";
                var cachedResult = await cacheService.GetAsync<Result<ReturnHistoryResponseDTO>>(cacheKey);
                if (cachedResult is not null) return cachedResult;

                var returns = await Task.Run(() => returnRepository.FindAll(x => x.UserId == request.UserId).ToList());
                if (returns == null || !returns.Any())
                {
                    return Result<ReturnHistoryResponseDTO>.Ok(new ReturnHistoryResponseDTO { TotalItems = 0, Items = new List<ReturnItemDTO>() });
                }

                var returnIds = returns.Select(ri => ri.OiId).Where(id => id != 0).ToList();
                if (!returnIds.Any())
                {
                    return Result<ReturnHistoryResponseDTO>.Ok(new ReturnHistoryResponseDTO { TotalItems = 0, Items = new List<ReturnItemDTO>() });
                }

                var orderItems = await Task.Run(() => orderItemRepository.FindAll(x => returnIds.Contains(x.Id)).ToList());
                if (orderItems == null || !orderItems.Any())
                {
                    return Result<ReturnHistoryResponseDTO>.Ok(new ReturnHistoryResponseDTO { TotalItems = 0, Items = new List<ReturnItemDTO>() });
                }

                var orderItemsDict = orderItems
                    .GroupBy(oi => oi.Id)
                    .Where(g => g.Any())
                    .ToDictionary(g => g.Key, g => g.First());

                var orderIds = returns.Select(x => x.OiId).Where(id => id != 0).ToList();
                var orders = await Task.Run(() => orderRepository.FindAll(x => orderIds.Contains(x.Id)).ToList());
                var ordersDict = orders
                    .GroupBy(o => o.Id)
                    .Where(g => g.Any())
                    .ToDictionary(g => g.Key, g => g.First().OrderCode);

                var variantIds = orderItems.Select(oi => oi.VariantId).Where(id => id != 0).ToList();
                var variants = await Task.Run(() => productVariantRepository.FindAll(x => variantIds.Contains(x.Id)).ToList());
                if (variants == null || !variants.Any())
                {
                    return Result<ReturnHistoryResponseDTO>.Ok(new ReturnHistoryResponseDTO { TotalItems = 0, Items = new List<ReturnItemDTO>() });
                }

                var variantsDict = variants
                    .GroupBy(v => v.Id)
                    .Where(g => g.Any())
                    .ToDictionary(g => g.Key, g => g.First());

                var productIds = variants.Select(v => v.ProductId).Where(id => id != 0).ToList();
                var products = await Task.Run(() => productRepository.FindAll(x => productIds.Contains(x.Id)).ToList());
                var productsDict = products
                    .GroupBy(p => p.Id)
                    .Where(g => g.Any())
                    .ToDictionary(g => g.Key, g => g.First());

                var variantIdsForImages = variants.Select(v => v.Id).Where(id => id != 0).ToList();
                var productImages = await Task.Run(() => productImageRepository.FindAll(x => variantIdsForImages.Contains(x.VariantId) && x.Position == 0).ToList());
                var productImageDict = productImages
                    .GroupBy(pi => pi.VariantId)
                    .Where(g => g.Any())
                    .ToDictionary(g => g.Key, g => g.First().Url);

                var productAttributes = await Task.Run(() => productAttributeRepository.FindAll(x => variantIdsForImages.Contains(x.VariantId)).ToList());
                var attributeValues = await Task.Run(() => attributeValueRepository.FindAll().ToList());
                var attributeValuesDict = attributeValues
                    .GroupBy(av => av.Id)
                    .Where(g => g.Any())
                    .ToDictionary(g => g.Key, g => g.First().Value);

                var returnDtos = returns.Select(r =>
                {
                    if (!orderItemsDict.TryGetValue(r.OiId, out var orderItem))
                        return null;

                    if (!variantsDict.TryGetValue(orderItem.VariantId, out var variant))
                        return null;

                    if (!productsDict.TryGetValue(variant.ProductId, out var product))
                        return null;

                    var attributes = productAttributes
                        .Where(pa => pa.VariantId == variant.Id)
                        .Select(pa => attributeValuesDict.TryGetValue(pa.AvId, out var value) ? value : "Unknown")
                        .Where(a => !string.IsNullOrEmpty(a));

                    return new ReturnItemDTO
                    {
                        ReturnId = r.Id,
                        VariantId = variant.Id,
                        Status = ((ItemStatus)orderItem.ItemStatus).ToString(),
                        Name = $"{product.Name} - {string.Join(" ", attributes)}",
                        ProductAttribute = string.Join(" - ", attributes),
                        ImageUrl = productImageDict.TryGetValue(variant.Id, out var imageUrl) 
                            ? fileService.GetFullPathFileServer(imageUrl)
                            : string.Empty,
                        Quantity = r.Quantity,
                        RefundAmount = r.RefundAmount,
                        CreatedAt = r.CreatedAt,
                        ProcessedAt = r.ProcessedAt,
                    };
                })
                .Where(x => x != null)
                .ToList();

                var totalItems = returnDtos.Count();
                var pagedDtos = returnDtos.Skip(request.Skip ?? 0).Take(request.Take ?? totalItems).ToList();
                var result = Result<ReturnHistoryResponseDTO>.Ok(new ReturnHistoryResponseDTO { TotalItems = totalItems, Items = pagedDtos });
                await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}