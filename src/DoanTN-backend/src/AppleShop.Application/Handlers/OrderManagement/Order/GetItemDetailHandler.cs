using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.OrderManagement.Order
{
    public class GetItemDetailHandler : IRequestHandler<GetDetailItemRequest, Result<OrderItemUserDTO>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IUserAddressRepository userAddressRepository;
        private readonly IFileService fileService;

        public GetItemDetailHandler(IOrderRepository orderRepository,
                                    IOrderItemRepository orderItemRepository,
                                    IProductVariantRepository productVariantRepository,
                                    IProductAttributeRepository productAttributeRepository,
                                    IAttributeValueRepository attributeValueRepository,
                                    IAttributeRepository attributeRepository,
                                    IProductRepository productRepository,
                                    IProductImageRepository productImageRepository,
                                    IUserAddressRepository userAddressRepository,
                                    IFileService fileService)
        {
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.productVariantRepository = productVariantRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.attributeRepository = attributeRepository;
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.userAddressRepository = userAddressRepository;
            this.fileService = fileService;
        }

        public async Task<Result<OrderItemUserDTO>> Handle(GetDetailItemRequest request, CancellationToken cancellationToken)
        {
            var orderItem = await orderItemRepository.FindByIdAsync(request.OiId, false);
            var order = await orderRepository.FindByIdAsync(orderItem.OrderId, false);

            var variant = await productVariantRepository.FindByIdAsync(orderItem.VariantId, false);
            var product = await productRepository.FindByIdAsync(variant.ProductId, false);
            var productImage = await productImageRepository.FindSingleAsync(x => x.VariantId == variant.Id && x.Position == 0);

            var productAttributes = productAttributeRepository.FindAll(x => x.VariantId == orderItem.VariantId).ToList();
            var attributeValues = attributeValueRepository.FindAll(x => productAttributes.Select(x => x.AvId).Contains(x.Id)).ToList();
            var attributes = attributeRepository.FindAll(x => true).ToDictionary(attr => attr.Id, attr => attr.Name);

            var fullAttributes = (from pa in productAttributes
                                where pa.VariantId == orderItem.VariantId
                                join av in attributeValues on pa.AvId equals av.Id
                                join attr in attributes on av.AttributeId equals attr.Key
                                select new
                                {
                                    AttributeName = attr.Value,
                                    AttributeValue = av.Value
                                }).ToList();

            string[] desiredOrder = { "kích thước", "màu sắc", "ram", "dung lượng", "cổng sạc" };
            var orderedAttributes = fullAttributes
                                    .OrderBy(attr =>
                                    {
                                        var lowerName = attr.AttributeName.ToLower();
                                        for (int i = 0; i < desiredOrder.Length; i++)
                                        {
                                            if (lowerName.Contains(desiredOrder[i])) return i;
                                        }
                                        return desiredOrder.Length;
                                    }).ToList();

            var userAddress = await userAddressRepository.FindByIdAsync(order.UserAddressId, false);

            var orderItemDto =  new OrderItemUserDTO
            {
                OiId = orderItem.Id,
                VariantId = orderItem.VariantId ?? 0,
                Status = ((ItemStatus)orderItem.ItemStatus).ToString(),
                Name = $"{product.Name} - {string.Join(" ", orderedAttributes.Select(a => a.AttributeValue))}",
                Image = productImage is not null ? fileService.GetFullPathFileServer(productImage.Url) : null,
                ProductAttribute = string.Join(" - ", orderedAttributes.Select(a => a.AttributeValue)),
                Quantity = orderItem.Quantity ?? 0,
                OriginalPrice = orderItem.OriginalPrice ?? 0,
                FinalPrice = orderItem.FinalPrice ?? 0,
                TotalPrice = orderItem.TotalPrice ?? 0,
                UserAddresses = new UserAddressDTO
                {
                    AddressId = userAddress.Id,
                    FullName = $"{userAddress.FirstName} {userAddress.LastName}",
                    PhoneNumber = userAddress.PhoneNumber,
                    Address = $"{userAddress.AddressLine}, {userAddress.Ward}, {userAddress.District}, {userAddress.Province}"
                }
            };

            return Result<OrderItemUserDTO>.Ok(orderItemDto);
        }
    }
}