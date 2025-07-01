using AppleShop.Share.Abstractions;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ReturnManagement.Return
{
    public class ChangeStatusReturnRequest : ICommand
    {
        [JsonIgnore]
        public int? Id { get; set; }
        public int? Status { get; set; }
    }
}