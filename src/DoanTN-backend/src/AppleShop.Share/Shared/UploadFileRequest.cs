using AppleShop.Share.Enumerations;

namespace AppleShop.Share.Shared
{
    public class UploadFileRequest
    {
        public string Content { get; set; }
        public string FileName { get; set; }
        public AssetType AssetType { get; set; }
        public Guid ReferenceId { get; set; }
        public string? Suffix { get; set; }
    }
}