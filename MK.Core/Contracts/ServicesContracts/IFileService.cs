using Microsoft.AspNetCore.Http;

namespace MK.Core.Contracts.ServicesContracts
{
    public interface IFileService
    {
        string GetFullUrl(string url);
        Task<string> SaveImageAsync(IFormFile image, string folderPath);
        void DeleteImageIfExists(string imageUrl);
    }
}
