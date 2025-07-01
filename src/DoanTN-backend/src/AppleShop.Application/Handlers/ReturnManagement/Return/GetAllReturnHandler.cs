using AppleShop.Application.Requests.DTOs.ReturnManagement.Return;
using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.ReturnManagement.Return;
using AppleShop.Application.Validators.ReturnManagement.Return;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReturnManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Domain.Entities.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;

namespace AppleShop.Application.Handlers.ReturnManagement.Return
{
    public class GetAllReturnHandler : IRequestHandler<GetAllReturnRequest, Result<ReturnListResponseDTO>>
    {
        private readonly IReturnRepository returnRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IOrderItemRepository orderItemRepository;
        private readonly IUserRepository userRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IAttributeValueRepository attributeValueRepository;
        private readonly IProductRepository productRepository;
        private readonly IFileService fileService;
        private readonly IProductImageRepository productImageRepository;
        private readonly IUserAddressRepository userAddressRepository;

        public GetAllReturnHandler(IReturnRepository returnRepository,
                                IOrderRepository orderRepository,
                                IOrderItemRepository orderItemRepository,
                                IUserRepository userRepository,
                                IProductAttributeRepository productAttributeRepository,
                                IProductVariantRepository productVariantRepository,
                                IAttributeValueRepository attributeValueRepository,
                                IProductRepository productRepository,
                                IFileService fileService,
                                IProductImageRepository productImageRepository,
                                IUserAddressRepository userAddressRepository)
        {
            this.returnRepository = returnRepository;
            this.orderRepository = orderRepository;
            this.orderItemRepository = orderItemRepository;
            this.userRepository = userRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.productVariantRepository = productVariantRepository;
            this.attributeValueRepository = attributeValueRepository;
            this.productRepository = productRepository;
            this.fileService = fileService;
            this.productImageRepository = productImageRepository;
            this.userAddressRepository = userAddressRepository;
        }

        public async Task<Result<ReturnListResponseDTO>> Handle(GetAllReturnRequest request, CancellationToken cancellationToken)
        {
            var validator = new GetAllReturnValidator();
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) AppleException.ThrowValidation(validationResult);

            var returns = returnRepository.FindAll().ToList();

            var orderItems = orderItemRepository.FindAll(x => returns.Select(ri => ri.OiId).Contains(x.Id)).ToDictionary(oi => oi.Id);
            var orders = orderRepository.FindAll(x => orderItems.Select(x => x.Value.OrderId).Contains(x.Id)).ToDictionary(o => o.Id);

            var users = userRepository.FindAll(x => returns.Select(x => x.UserId).Contains(x.Id)).ToDictionary(u => u.Id, u => u.Email);
            var userAddresses = userAddressRepository.FindAll(x => orders.Select(x => x.Value.UserAddressId).Contains(x.Id)).ToDictionary(ua => ua.Id);

            var variants = productVariantRepository.FindAll(x => orderItems.Select(oi => oi.Value.VariantId).Contains(x.Id)).ToDictionary(v => v.Id, v => v);
            var products = productRepository.FindAll(x => variants.Values.Select(v => v.ProductId).Contains(x.Id)).ToDictionary(p => p.Id, p => p);
            var productImage = productImageRepository.FindAll(x => variants.Select(v => v.Value.Id).Contains(x.VariantId) && x.Position == 0).GroupBy(x => x.VariantId).ToDictionary(g => g.Key, g => g.First().Url);

            var productAttributes = productAttributeRepository.FindAll(x => variants.Values.Select(v => v.Id).Contains(x.VariantId)).ToList();
            var attributeValues = (attributeValueRepository.FindAll() ?? Enumerable.Empty<AttributeValue>()).GroupBy(av => av.Id).ToDictionary(g => g.Key, g => g.First().Value);

            var returnDtos = returns.Select(r =>
            {
                if (!orderItems.TryGetValue(r.OiId, out var orderItem) ||
                    !orders.TryGetValue(orderItem.OrderId, out var order) ||
                    !users.TryGetValue(r.UserId, out var email) ||
                    !variants.TryGetValue(orderItem.VariantId, out var variant) ||
                    !products.TryGetValue(variant.ProductId, out var product) ||
                    !productImage.TryGetValue(variant.Id, out var imageUrl) ||
                    !userAddresses.TryGetValue(order.UserAddressId, out var address))
                {
                    return null; // skip nếu thiếu dữ liệu
                }

                var name = $"{product.Name} - " + string.Join(" ",
                    productAttributes
                        .Where(pa => pa.VariantId == variant.Id)
                        .Select(pa => attributeValues.TryGetValue(pa.AvId, out var val) ? val : "Unknown"));

                return new ReturnDTO
                {
                    ReturnId = r.Id,
                    OrderCode = order.OrderCode,
                    Email = email,
                    Reason = r.Reason,
                    VariantId = variant.Id,
                    Name = name,
                    ImageUrl = fileService.GetFullPathFileServer(imageUrl),
                    ReturnUrl = fileService.GetFullPathFileServer(r.Url),
                    Status = ((ReturnStatus)r.Status).ToString(),
                    Quantity = r.Quantity,
                    RefundAmount = r.RefundAmount,
                    UserAddresses = new UserAddressDTO
                    {
                        AddressId = address.Id,
                        FullName = $"{address.FirstName} {address.LastName}",
                        PhoneNumber = address.PhoneNumber,
                        Address = $"{address.AddressLine}, {address.Ward}, {address.District}, {address.Province}"
                    },
                    CreatedAt = r.CreatedAt,
                    ProcessedAt = r.ProcessedAt,
                    AccountName = r.AccountName,
                    AccountNumber = r.AccountNumber,
                    BankName = r.BankName,
                    PhoneNumber = r.PhoneNumber,
                    ReturnType = r.ReturnType,
                };
            }).Where(dto => dto != null).OrderByDescending(x => x.ReturnId).ToList();

            var totalItems = returnDtos.Count();
            var pagedDtos = returnDtos.Skip(request.Skip ?? 0).Take(request.Take ?? totalItems).ToList();
            return Result<ReturnListResponseDTO>.Ok(new ReturnListResponseDTO { TotalItems = totalItems, Items = pagedDtos });
        }
    }
}