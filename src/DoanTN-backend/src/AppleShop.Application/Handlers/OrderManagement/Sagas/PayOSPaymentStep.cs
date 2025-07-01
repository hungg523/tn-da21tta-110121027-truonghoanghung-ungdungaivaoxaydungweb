using AppleShop.Application.Requests.DTOs.OrderManagement.Saga;
using AppleShop.Application.Requests.OrderManagement.Order;
using AppleShop.Domain.Abstractions.IRepositories.OrderManagement;
using AppleShop.Domain.Abstractions.IRepositories.ProductManagement;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Enumerations;
using AppleShop.Share.Exceptions;
using Net.payOS.Types;
using System.Linq;
using System.Net;

namespace AppleShop.Application.Handlers.OrderManagement.Sagas
{
    public class PayOSPaymentStep : ISagaStep
    {
        private readonly IPayOSService vnpayService;
        private readonly IOrderRepository orderRepository;
        private readonly CreateOrderRequest request;
        private readonly OrderSagaDTO orderSagaDTO;
        private readonly IProductVariantRepository productVariantRepository;
        private readonly IProductRepository productRepository;
        private readonly IProductAttributeRepository productAttributeRepository;
        private readonly IAttributeValueRepository attributeValueRepository;

        private string? paymentUrl;

        public PayOSPaymentStep(IPayOSService vnpayService,
                                IOrderRepository orderRepository,
                                CreateOrderRequest request,
                                OrderSagaDTO orderSagaDTO,
                                IProductVariantRepository productVariantRepository,
                                IProductRepository productRepository,
                                IProductAttributeRepository productAttributeRepository,
                                IAttributeValueRepository attributeValueRepository)
        {
            this.vnpayService = vnpayService;
            this.orderRepository = orderRepository;
            this.request = request;
            this.orderSagaDTO = orderSagaDTO;
            this.productVariantRepository = productVariantRepository;
            this.productRepository = productRepository;
            this.productAttributeRepository = productAttributeRepository;
            this.attributeValueRepository = attributeValueRepository;
        }

        public string? PaymentUrl => paymentUrl;

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (request.Payment.ToLower() != Payment.PayOS.ToString().ToLower()) return;

            var order = await orderRepository.FindSingleAsync(x => x.OrderCode == orderSagaDTO.Code, false, cancellationToken, oi => oi.OrderItems!);
            var variants = productVariantRepository.FindAll(x => order.OrderItems.Select(x => x.VariantId).Contains(x.Id)).ToList();
            var variantDict = variants.ToDictionary(p => p.Id);
            var products = productRepository.FindAll(x => variants.Select(p => p.ProductId).Contains(x.Id)).ToDictionary(p => p.Id);
            var productAttributes = productAttributeRepository.FindAll(x => variants.Select(p => p.Id).Contains(x.VariantId)).ToList();
            //var productAttributes = productAttributeRepository.FindAll(x => variants.Select(p => p.Id).Contains(x.VariantId)).ToDictionary(pa => pa.Id);
            var attributeValues = attributeValueRepository.FindAll(x => productAttributes.Select(x => x.AvId).Contains(x.Id)).ToDictionary(av => av.Id, av => av.Value);

            List<ItemData> items = order.OrderItems!.Select(oi => new ItemData
            (
                name: $"{products[variantDict[oi.VariantId.Value].ProductId].Name} - {string.Join(" ", productAttributes.Where(pa => pa.VariantId == oi.VariantId).Select(pa => attributeValues.TryGetValue(pa.AvId, out var value) ? value : "Unknown"))}",
                quantity: oi.Quantity.Value,
                price: (int)oi.TotalPrice.Value
            )).ToList();
            paymentUrl = await vnpayService.CreatePaymentUrlAsync(order.OrderCode, order.TotalAmount.Value, items);

            if (paymentUrl is null) AppleException.ThrowException((int)HttpStatusCode.BadRequest, "Can not create URL PayOS.");
            orderSagaDTO.PaymentUrl = paymentUrl;
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}