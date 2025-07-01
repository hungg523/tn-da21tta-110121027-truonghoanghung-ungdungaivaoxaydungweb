using AppleShop.Application.Requests.DTOs.ProductManagement.Product;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace AppleShop.Application.Services
{
    public class ProductService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;
        private readonly ICacheService cacheService;

        public ProductService(HttpClient httpClient, IConfiguration configuration, ICacheService cacheService)
        {
            this.httpClient = httpClient;
            baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
            this.cacheService = cacheService;
        }

        public async Task<Result<List<ProductFullDTO>>> GetAllProducts()
        {
            var cacheKey = "all_products";
            var cached = await cacheService.GetAsync<Result<List<ProductFullDTO>>>(cacheKey);
            if (cached != null) return cached;
            try
            {
                var result = await httpClient.GetFromJsonAsync<Result<List<ProductFullDTO>>>($"{baseUrl}/api/v1/product-variant/get-all?isActived=1");
                await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
                return result;
            }
            catch
            {
                return new List<ProductFullDTO>();
            }
        }

        public async Task<Result<List<ProductFullDTO>>> GetFlashSaleProducts()
        {
            var cacheKey = "flash_sale_products";
            var cached = await cacheService.GetAsync<Result<List<ProductFullDTO>>>(cacheKey);
            if (cached != null) return cached;
            try
            {
                var result = await httpClient.GetFromJsonAsync<Result<List<ProductFullDTO>>>($"{baseUrl}/api/v1/product-variant/flash-sale");
                await cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
                return result;
            }
            catch
            {
                return new List<ProductFullDTO>();
            }
        }
    }
} 