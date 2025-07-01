using System.Collections.Generic;

namespace AppleShop.Application.Requests.DTOs.OrderManagement.Order
{
    public class OrderListResponseDTO
    {
        public int TotalItems { get; set; }
        public List<OrderDTO> Items { get; set; } = new List<OrderDTO>();
    }
} 