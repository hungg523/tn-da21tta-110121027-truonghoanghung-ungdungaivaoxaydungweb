using AppleShop.Application.Requests.DTOs.OrderManagement.Order;
using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Application.Validators.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MassTransit;
using MediatR;
using Entities = AppleShop.Domain.Entities;

namespace AppleShop.Application.Handlers.OrderManagement.Order
{
    public class GetOrderDetailHandler : IRequestHandler<GetOrderDetailRequest, Result<OrderFullDTO>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IUserRepository userRepository;
        private readonly IUserAddressRepository userAddressRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IAttributeRepository attributeRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductImageRepository productImageRepository;
        private readonly IFileService fileService;

        public GetOrderDetailHandler(IOrderRepository orderRepository,
                                    IProductVariantRepository productVariantRepository,
                                    IOrderItemRepository orderItemRepository,
                                    IUserRepository userRepository,
                                    IUserAddressRepository userAddressRepository,
                                    IProductAttributeRepository productAttributeRepository,
                                    IAttributeValueRepository attributeValueRepository,
                                    IAttributeRepository attributeRepository,
                                    IProductRepository productRepository,
                                    IProductImageRepository productImageRepository,
                                    IFileService fileService)
        {
            this.orderRepository = orderRepository;
            this.productVariantRepository = productVariantRepository;
            this.orderItemRepository = orderItemRepository;
            this.userRepository = userRepository;
            this.userAddressRepository = userAddressRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.attributeRepository = attributeRepository;
            this.productRepository = productRepository;
            this.productImageRepository = productImageRepository;
            this.fileService = fileService;
        }

        public async Task<Result<OrderFullDTO>> Handle(GetOrderDetailRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetOrderDetailValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var order = await orderRepository.FindSingleAsync(x => x.Id == request.OrderId, false, cancellationToken, oi => oi.OrderItems);
            if (order is null) AppleException.ThrowNotFound(typeof(Entities.OrderManagement.Order));

            var totalQuantity = orderItemRepository.FindAll(x => x.OrderId == order.Id, false).Count();

            var variants = productVariantRepository.FindAll(x => order.OrderItems.Select(x => x.VariantId).Contains(x.Id)).ToDictionary(p => p.Id, p => p);
            var products = productRepository.FindAll(x => variants.Select(p => p.Value.ProductId).Contains(x.Id)).ToDictionary(p => p.Id, p => p);
            var productImage = await productImageRepository.FindSingleAsync(x => variants.Select(v => v.Value.Id).Contains(x.VariantId) && x.Position == 0);

            var productAttributes = productAttributeRepository.FindAll(x => variants.Select(p => p.Value.Id).Contains(x.VariantId)).ToList();
            var attributeValues = attributeValueRepository.FindAll(x => productAttributes.Select(x => x.AvId).Contains(x.Id)).ToList();
            var attributes = attributeRepository.FindAll(x => true).ToDictionary(attr => attr.Id, attr => attr.Name);

            var users = userRepository.FindAll(x => x.Id == order.UserId).ToDictionary(u => u.Id, u => u.Email);
            var userAddress = await userAddressRepository.FindByIdAsync(order.UserAddressId);

            string[] desiredOrder = { "kích thước", "màu sắc", "ram", "dung lượng", "cổng sạc" };

            var orderDto = new OrderFullDTO
            {
                OrderId = order.Id,
                Code = order.OrderCode,
                Email = order.UserId.HasValue && users.ContainsKey(order.UserId.Value) ? users[order.UserId] : null,
                Status = ((OrderStatus)order.Status).ToString(),
                TotalQuantity = totalQuantity,
                TotalAmount = order.TotalAmount ?? 0,
                Items = order.OrderItems.Select(oi =>
                {
                    var fullAttributes = (from pa in productAttributes
                                        where pa.VariantId == oi.VariantId
                                        join av in attributeValues on pa.AvId equals av.Id
                                        join attr in attributes on av.AttributeId equals attr.Key
                                        select new
                                        {
                                            AttributeName = attr.Value,
                                            AttributeValue = av.Value
                                        }).ToList();

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

                    return new OrderItemUserDTO
                    {
                        OiId = oi.Id,
                        VariantId = oi.VariantId ?? 0,
                        Status = ((ItemStatus)oi.ItemStatus).ToString(),
                        Name = variants.ContainsKey(oi.VariantId ?? 0)
                                      ? $"{products[variants[oi.VariantId.Value].ProductId].Name} - {string.Join(" ", orderedAttributes.Select(a => a.AttributeValue))}"
                                      : null,
                        Image = fileService.GetFullPathFileServer(productImage.Url),
                        ProductAttribute = string.Join(" - ", orderedAttributes.Select(a => a.AttributeValue)),
                        Quantity = oi.Quantity ?? 0,
                        OriginalPrice = oi.OriginalPrice ?? 0,
                        FinalPrice = oi.FinalPrice ?? 0,
                        TotalPrice = oi.TotalPrice ?? 0,
                        UserAddresses = new UserAddressDTO
                        {
                            AddressId = userAddress.Id,
                            FullName = $"{userAddress.FirstName} {userAddress.LastName}",
                            PhoneNumber = userAddress.PhoneNumber,
                            Address = $"{userAddress.AddressLine}, {userAddress.Ward}, {userAddress.District}, {userAddress.Province}"
                        }
                    };
                }).ToList(),
            };
            return Result<OrderFullDTO>.Ok(orderDto);
        }
    }
}