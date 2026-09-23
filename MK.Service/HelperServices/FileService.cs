using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MK.Core.Contracts.ServicesContracts;

namespace MK.Service.HelperServices
{
    public class FileService : IFileService
    { 
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public FileService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        public string GetFullUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? string.Empty;

            return $"{baseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
        }

        public async Task<string> SaveImageAsync(IFormFile image, string folderPath)
        {
            if (image is null || image.Length == 0)
                throw new Exception("Invalid image.");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };

            if (!allowedTypes.Contains(image.ContentType))
                throw new Exception("Only JPG, PNG, and WEBP images are allowed.");

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            var fileName = $"{Guid.NewGuid()}{extension}";

            var uploadsFolder = Path.Combine(_env.WebRootPath, folderPath);

            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);

            await image.CopyToAsync(stream);

            return $"/{folderPath.Replace("\\", "/")}/{fileName}";
        }

        public void DeleteImageIfExists(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            var imagePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/')
                .Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            if (File.Exists(imagePath))
                File.Delete(imagePath);
        }
    }
}
