using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using VitalOps.API.Services.Interfaces;
using VitalOps.API.Services.Results;
using Error = VitalOps.API.Services.Results.Error;

namespace VitalOps.API.Services.Implementations
{
    public class CloudinaryFileService : IFileService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryFileService> _logger;

        public CloudinaryFileService(Cloudinary cloudinary, ILogger<CloudinaryFileService> logger)
        {
            _cloudinary = cloudinary;
            _logger = logger;
        }

        public async Task<Result<string>> UploadFile(IFormFile file, string? uploadedFilePath, string? uploadSubDir)
        {
            var fileValidationResult = IsFileValid(file);
            if (!fileValidationResult.IsSucceeded)
            {
                return Result<string>.Failure(fileValidationResult.Errors[0]);
            }

            if (!string.IsNullOrEmpty(uploadedFilePath))
            {
                await DeleteFile(uploadedFilePath);
            }

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = uploadSubDir ?? "vitalops-uploads",
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary Error: {error}", uploadResult.Error.Message);
                return Result<string>.Failure(Error.File.ValidationFailed());
            }

            return Result<string>.Success(uploadResult.SecureUrl.ToString());
        }

        public async Task<Result> DeleteFile(string filePath)
        {

            var publicId = ExtractPublicIdFromUrl(filePath);

            if (string.IsNullOrEmpty(publicId)) return Result.Success();

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Result != "ok")
            {
                _logger.LogWarning("Cloudinary delete failed for ID: {id}", publicId);
            }

            return Result.Success();
        }

        private Result IsFileValid(IFormFile file)
        {
            var fileSize = file.Length;

            if (fileSize == 0)
                return Result.Failure(Error.File.Empty());

            var maxFileSize = 5 * 1024 * 1024;

            if (fileSize > maxFileSize)
                return Result.Failure(Error.File.TooLarge(maxFileSize));

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Result.Failure(Error.File.UnsupportedExtension(extension));
            return Result.Success();
        }

        private string? ExtractPublicIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || !url.Contains("res.cloudinary.com")) return null;

            var uri = new Uri(url);
            var pathSegments = uri.AbsolutePath.Split('/');
            var fileNameWithExtension = pathSegments.Last();
            var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);

            var folder = pathSegments[pathSegments.Length - 2];
            return folder != "upload" ? $"{folder}/{fileName}" : fileName;
        }

        Result IFileService.DeleteFile(string filePath)
        {
            DeleteFile(filePath).Wait();
            return Result.Success();
        }
    }
}