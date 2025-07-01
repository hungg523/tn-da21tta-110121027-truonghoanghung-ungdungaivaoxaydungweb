using System.Collections.Generic;

namespace AppleShop.Application.Requests.DTOs.OrderManagement.Order
{
    public class OrderItemListResponseDTO
    {
        public int TotalItems { get; set; }
        public List<OrderItemFullDTO> Items { get; set; } = new List<OrderItemFullDTO>();
    }
} 