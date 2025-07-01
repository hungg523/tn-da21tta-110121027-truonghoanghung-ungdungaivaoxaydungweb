namespace AppleShop.Share.Shared
{
    public class PaginationQuery
    {
        public string? SortBy { get; set; }
        public bool? IsDescending { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

        public PaginationQuery() { }

        public PaginationQuery(string? sortBy = null, bool? isDescending = null, int? pageNumber = null, int? pageSize = null)
        {
            SortBy = sortBy;
            IsDescending = isDescending ?? true;
            PageNumber = Math.Max(pageNumber ?? 1, 1);
            PageSize = Math.Max(pageSize ?? 10, 10);
        }
    }
}