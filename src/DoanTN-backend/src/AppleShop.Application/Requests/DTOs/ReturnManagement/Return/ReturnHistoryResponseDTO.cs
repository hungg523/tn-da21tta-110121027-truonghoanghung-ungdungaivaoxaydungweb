using System.Collections.Generic;

namespace AppleShop.Application.Requests.DTOs.ReturnManagement.Return
{
    public class ReturnHistoryResponseDTO
    {
        public int TotalItems { get; set; }
        public List<ReturnItemDTO> Items { get; set; } = new List<ReturnItemDTO>();
    }
} 