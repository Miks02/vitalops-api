using Microsoft.AspNetCore.Http.Features;
using MixxFit.API.Services.Interfaces;
using MixxFit.API.Services.Results;
using MixxFit.API.Extensions;

namespace MixxFit.API.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _web;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment web, ILogger<FileService> logger)
        {
            _web = web;
            _logger = logger;
        }

        public async Task<Result<string>> UploadFile(IFormFile file, string? uploadedFilePath, string? uploadSubDir)
        {
            var fileValidationResult = IsFileValid(file);

            if (!fileValidationResult.IsSucceeded)
            {
                _logger.LogWarning("{error}", fileValidationResult.Errors[0].Description);
                return Result<string>.Failure(Error.File.ValidationFailed());
            }

            var uploadDir = "Uploads";

            if (!string.IsNullOrEmpty(uploadSubDir))
            {
                uploadSubDir = uploadSubDir.TrimStart('/');
                uploadDir = Path.Combine(uploadDir, uploadSubDir).Replace('\\', '/') + '/';
            }

            var uploadsDirPath = Path.Combine(_web.WebRootPath, uploadDir.TrimStart());

            if (!Directory.Exists(uploadsDirPath))
                Directory.CreateDirectory(uploadsDirPath);

            var sanitizedFileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
            var filePath = Path.Combine(uploadsDirPath, uniqueFileName);

            if (!string.IsNullOrEmpty(uploadedFilePath))
                DeleteFile(uploadedFilePath);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return Result<string>.Success(uploadDir + uniqueFileName);

        }

        public Result DeleteFile(string filePath)
        {
            var oldFilePath = Path.Combine(_web.WebRootPath,
                filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(oldFilePath))
            {
                _logger.LogWarning("File does not exist");
                Result.Failure(Error.Resource.NotFound("File does not exist"));
            }

            try
            {
                File.Delete(oldFilePath);
            }
            catch (IOException ex)
            {
                _logger.LogError("Unexpected error happened while trying to delete the file. {ex}", ex);
                throw;
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
    }
}
