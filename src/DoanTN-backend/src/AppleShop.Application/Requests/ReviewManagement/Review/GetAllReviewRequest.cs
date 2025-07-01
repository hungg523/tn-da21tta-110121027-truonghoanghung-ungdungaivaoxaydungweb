using AppleShop.Application.Requests.DTOs.ReviewManagement.Review;
using AppleShop.Share.Abstractions;
using AppleShop.Share.Shared;
using System.Text.Json.Serialization;

namespace AppleShop.Application.Requests.ReviewManagement.Review
{
    public class GetAllReviewRequest : IQuery<PagedList<ProductReviewDTO>>
    {
        [JsonIgnore]
        public int? VariantId { get; set; }
        public int? Star { get; set; }
        public bool? IsFilterByImage { get; set; }
        public bool? IsDeleted { get; set; }

        public PaginationQuery paginationQuery { get; set; }

        public GetAllReviewRequest(PaginationQuery paginationQuery)
        {
            this.paginationQuery = paginationQuery;
        }
    }
}