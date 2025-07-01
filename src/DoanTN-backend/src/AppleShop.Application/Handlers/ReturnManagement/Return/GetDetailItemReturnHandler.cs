using AppleShop.Application.Requests.DTOs.ReturnManagement.Return;
using AppleShop.Application.Requests.DTOs.UserManagement.User;
using AppleShop.Application.Requests.ReturnManagement.Return;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Domain.Abstractions.IRepositories.ReturnManagement;
using AppleShop.Domain.Abstractions.IRepositories.UserManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Shared;
using MediatR;
using Entities = AppleShop.Domain.Entities.ReturnManagement;

namespace AppleShop.Application.Handlers.ReturnManagement.Return
{
    public class GetDetailItemReturnHandler : IRequestHandler<GetDetailItemReturnRequest, Result<ReturnDetailDTO>>
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
        private readonly IUserAddressRepository userAddressRepository;

        public GetDetailItemReturnHandler(IReturnRepository returnRepository,
                                          IOrderRepository orderRepository,
                                          IOrderItemRepository orderItemRepository,
                                          IProductAttributeRepository productAttributeRepository,
                                          IProductVariantRepository productVariantRepository,
                                          IAttributeValueRepository attributeValueRepository,
                                          IProductRepository productRepository,
                                          IProductImageRepository productImageRepository,
                                          IFileService fileService,
                                          IUserAddressRepository userAddressRepository)
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
            this.userAddressRepository = userAddressRepository;
        }

        public async Task<Result<ReturnDetailDTO>> Handle(GetDetailItemReturnRequest request, CancellationToken cancellationToken)
        {
            var @return = await returnRepository.FindByIdAsync(request.ReturnId, false);
            if (@return is null) AppleException.ThrowNotFound(typeof(Entities.Return));

            var orderItem = await orderItemRepository.FindByIdAsync(@return.OiId, false);
            var order = await orderRepository.FindByIdAsync(orderItem.OrderId, false);

            var variant = await productVariantRepository.FindByIdAsync(orderItem.VariantId, false);
            var product = await productRepository.FindByIdAsync(variant.ProductId, false);
            var productImage = await productImageRepository.FindSingleAsync(x => x.VariantId == variant.Id && x.Position == 0, false);

            var productAttributes = productAttributeRepository.FindAll(x => x.VariantId == variant.Id).ToList();
            var attributeValues = attributeValueRepository.FindAll().ToDictionary(av => av.Id, av => av.Value);

            var userAddress = await userAddressRepository.FindByIdAsync(order.UserAddressId, false);

            var returnDto = new ReturnDetailDTO
            {
                ReturnId = @return.Id,
                VariantId = variant.Id,
                Status = ((ItemStatus)orderItem.ItemStatus).ToString(),
                Name = $"{product.Name} - {string.Join(" ", productAttributes.Where(pa => pa.VariantId == orderItem.VariantId)
                                                                            .Select(pa => attributeValues.TryGetValue(pa.AvId, out var value) ? value : "Unknown"))}",
                ProductAttribute = string.Join(" - ", productAttributes
                                      .Where(pa => pa.VariantId == orderItem.VariantId)
                                      .Select(pa => attributeValues.TryGetValue(pa.AvId, out var value) ? value : "Unknown")),
                ImageUrl = fileService.GetFullPathFileServer(productImage.Url),
                ReturnUrl = fileService.GetFullPathFileServer(@return.Url),
                Quantity = @return.Quantity,
                RefundAmount = @return.RefundAmount,
                UserAddresses = new UserAddressDTO
                {
                    AddressId = userAddress.Id,
                    FullName = $"{userAddress.FirstName} {userAddress.LastName}",
                    PhoneNumber = userAddress.PhoneNumber,
                    Address = $"{userAddress.AddressLine}, {userAddress.Ward}, {userAddress.District}, {userAddress.Province}"
                },
                CreatedAt = @return.CreatedAt,
                ProcessedAt = @return.ProcessedAt,
                AccountName = @return.AccountName,
                AccountNumber = @return.AccountNumber,
                BankName = @return.BankName,
                PhoneNumber = @return.PhoneNumber,
                ReturnType = @return.ReturnType,
            };

            return Result<ReturnDetailDTO>.Ok(returnDto);
        }
    }
}