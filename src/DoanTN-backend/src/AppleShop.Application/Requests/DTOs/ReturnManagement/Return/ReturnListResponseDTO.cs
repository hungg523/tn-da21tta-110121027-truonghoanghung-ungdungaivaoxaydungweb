using System.Collections.Generic;

namespace AppleShop.Application.Requests.DTOs.ReturnManagement.Return
{
    public class ReturnListResponseDTO
    {
        public int TotalItems { get; set; }
        public List<ReturnDTO> Items { get; set; } = new List<ReturnDTO>();
    }
} 