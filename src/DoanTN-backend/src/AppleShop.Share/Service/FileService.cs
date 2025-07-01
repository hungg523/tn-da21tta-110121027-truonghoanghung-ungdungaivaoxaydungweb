using AppleShop.Share.Abstractions;
using AppleShop.Share.Exceptions;
using AppleShop.Share.Settings;
using AppleShop.Share.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Net;

namespace AppleShop.Share.Service
{
    public class FileService : IFileService
    {
        private readonly string fileDirectoryPath;
        private readonly HostSettings hostSetting;
        private readonly UploadSettings uploadedSettings;

        private const long MAX_FILE_SIZE_IMAGE = 2 * 1024 * 1024;
        private const long MAX_FILE_SIZE_VIDEO = 5 * 1024 * 1024;

        private long GetMaxFileSizeForExtension(string extension)
        {
            if (new[] { ".mp4", ".avi", ".mov", ".mkv", ".flv" }.Contains(extension.ToLower()))
                return MAX_FILE_SIZE_VIDEO;
            return MAX_FILE_SIZE_IMAGE;
        }

        // use data from db
        private readonly List<string> permittedExtensions = new List<string>
        {
            ".png", ".jpg", ".jpeg", ".pdf", ".webp",
            ".mp4", ".avi", ".mov", ".mkv", ".flv"
        };

        private static readonly List<FileSignature> FileSignatures =
        [
            new FileSignature(".jpeg", [0xFF, 0xD8]), // JPEG
            new FileSignature(".png", [0x89, 0x50, 0x4E, 0x47]), // PNG
            new FileSignature(".pdf", [0x25, 0x50, 0x44, 0x46]), // PDF
            new FileSignature(".gif", [0x47, 0x49, 0x46, 0x38]), // GIF
            new FileSignature(".webp", [
                0x52, 0x49, 0x46, 0x46, // RIFF
                null, null, null, null, // xx xx xx xx (bỏ qua)
                0x57, 0x45, 0x42, 0x50 // WEBP
            ]),
            new FileSignature(".mp4", [0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70]), // MP4
            new FileSignature(".mp4", [0x00, 0x00, 0x00, null, 0x66, 0x74, 0x79, 0x70]), // MP4
            new FileSignature(".mp4", [0x00, 0x00, 0x00, null, 0x6D, 0x70, 0x34, 0x32]), // MP4
            new FileSignature(".mp4", [0x00, 0x00, 0x00, null, 0x69, 0x73, 0x6F, 0x6D]), // MP4
            new FileSignature(".mp4", [0x00, 0x00, 0x00, null, 0x66, 0x74, 0x79, 0x70]), // MP4
            new FileSignature(".mp4", [0x00, 0x00, 0x00, null, 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D]), // MP4
            new FileSignature(".avi", [0x52, 0x49, 0x46, 0x46, null, null, null, null, 0x41, 0x56, 0x49]), // AVI
            new FileSignature(".mov", [0x00, 0x00, 0x00, null, 0x6D, 0x6F, 0x6F, 0x76]), // MOV
            new FileSignature(".mkv", [0x1A, 0x45, 0xDF, 0xA3]) // MKV
        ];

        public FileService(IWebHostEnvironment env,
                           IOptions<HostSettings> hostSetting,
                           IOptions<UploadSettings> uploadedSettings)
        {
            fileDirectoryPath = Path.GetFileName(env.WebRootPath).Replace("\\", "/");
            this.hostSetting = hostSetting.Value;
            this.uploadedSettings = uploadedSettings.Value;
        }

        private string GetFileExtensionFromBase64(string base64Content)
        {
            // Convert base64 into byte array
            byte[] fileBytes = Convert.FromBase64String(base64Content);

            //Console.WriteLine("First 16 bytes:");
            //for (int i = 0; i < Math.Min(16, fileBytes.Length); i++)
            //{
            //    Console.Write($"{fileBytes[i]:X2} ");
            //}

            // Compare a length first bytes of fileBytes with signature key to get file extension
            return FileSignatures.FirstOrDefault(signature =>

                       // File length must be longer than signature length
                       fileBytes.Length >= signature.Signature.Length &&

                       // Take a length bytes of signature key from fileBytes and compare sequence equal with signature keys
                       MatchesSignature(fileBytes, signature.Signature))?.Extension

                   // If there is no matching signature, throw exception
                   ?? throw new InvalidOperationException("Unknown file format.");
        }

        private string GetFilePath(string entity, string fileExtension)
        {
            string basePath = fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mov" || fileExtension == ".mkv"
                ? uploadedSettings.BasePaths.Videos
                : uploadedSettings.BasePaths.Images;

            string fullRelativePath = Path.Combine(Path.GetFileName(fileDirectoryPath), basePath, entity);
            if (!Directory.Exists(fullRelativePath)) Directory.CreateDirectory(fullRelativePath);

            return fullRelativePath.Replace("\\", "/");
        }

        private string GetRelativeFilePath(string relativePath, string fileName)
        {
            var a = Path.Combine(relativePath, fileName).Replace("\\", "/");
            return a;
        }

        private async Task SaveFile(IFormFile file, string filePath)
        {
            // Write file to directory
            await using FileStream stream = new(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
        }

        private IFormFile ConvertBase64ToFormFile(string base64Content, string fileName)
        {
            byte[]? fileBytes = Convert.FromBase64String(base64Content);
            if (fileBytes == null || fileBytes.Length == 0) AppleException.ThrowException((int)HttpStatusCode.BadRequest, "Invalid File.");

            string extension = Path.GetExtension(fileName).ToLower();
            long maxSize = GetMaxFileSizeForExtension(extension);
            if (fileBytes.Length > maxSize) AppleException.ThrowException((int)HttpStatusCode.BadRequest, "File too large.");

            MemoryStream stream = new(fileBytes);
            return new FormFile(stream, 0, fileBytes.Length, "file", fileName);
        }

        private bool IsExtensionPermitted(string extension) => permittedExtensions.Contains(extension);

        private static bool MatchesSignature(byte[] fileBytes, byte?[] signature)
        {
            for (int i = 0; i < signature.Length; i++)
            {
                // Skip null
                if (signature[i] == null)
                    continue;
                if (fileBytes[i] != signature[i])
                    return false;
            }

            return true;
        }

        public async Task<string> UploadFileAsync(UploadFileRequest uploadFileRequest)
        {
            string pathResult = string.Empty;
            string finalPathResult = string.Empty;
            try
            {
                //UploadFileRequest uploadFileRequest = context.Message;
                string extension = GetFileExtensionFromBase64(uploadFileRequest.Content);
                string entity = ConvertEnumToFolderName(uploadFileRequest.AssetType);
                string fileNameWithoutExt = FileNameGenerator.Generate(entity, uploadFileRequest.Suffix);
                string fileName = $"{fileNameWithoutExt}{extension}";

                // Check extension is permitted
                if (!IsExtensionPermitted(extension)) AppleException.ThrowException((int)HttpStatusCode.BadRequest, "Invalid File or Extiension not permitted.");

                IFormFile file = ConvertBase64ToFormFile(uploadFileRequest.Content, fileName);

                string relativePath = GetFilePath(entity, extension);
                string filePath = Path.Combine(relativePath, fileName).Replace("\\", "/");
                // Write file to directory
                await SaveFile(file, filePath);

                pathResult = GetRelativeFilePath(relativePath, fileName);
                finalPathResult = pathResult.Replace($"{Path.GetFileName(fileDirectoryPath)}/", "");
            }
            catch (Exception)
            {
                //throw;
                finalPathResult = "";
            }

            return finalPathResult;
        }

        public static class FileNameGenerator
        {
            public static string Generate(string entity, string suffix)
            {
                string baseName = $"{entity}-{suffix}";
                return baseName;
            }
        }

        private string ConvertEnumToFolderName(Enum assetType)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(assetType.ToString(), "(?<!^)([A-Z])", "_$1")
                .ToLower();
        }

        public string GetFullPathFileServer(string? filePath)
        {
            return !string.IsNullOrWhiteSpace(filePath) ? string.Format("{0}{1}", hostSetting.Url, filePath) : null;
        }
    }
}