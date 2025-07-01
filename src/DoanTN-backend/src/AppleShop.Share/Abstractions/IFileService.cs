using AppleShop.Share.Enumerations;
using AppleShop.Share.Shared;

namespace AppleShop.Share.Abstractions
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(UploadFileRequest uploadFileSetting);
        string GetFullPathFileServer(string? filePath);
    }
}