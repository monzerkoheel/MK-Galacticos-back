using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MK.Core.Contracts.ServicesContracts;
using MK.Core.HelperModels;

namespace MK.Service.HelperServices
{
    public class AiModerationService : IAiModerationService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        public AiModerationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AiModerationResult> ModerateFanRequestAsync(
            string name,
            string description,
            string imagePath,
            string? lang,
            CancellationToken cancellationToken = default)
        {
            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(name), "name");
            form.Add(new StringContent(description), "description");

            if (!string.IsNullOrWhiteSpace(lang))
                form.Add(new StringContent(lang), "lang");

            // The stream is owned (and disposed) by the form content.
            var imageContent = new StreamContent(File.OpenRead(imagePath));
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(imagePath));
            form.Add(imageContent, "image", Path.GetFileName(imagePath));

            using var response = await _httpClient.PostAsync("moderate/fan-request", form, cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<AiApiResponse>(JsonOptions, cancellationToken)
                       ?? throw new InvalidOperationException("Empty response from the AI moderation API.");

            return new AiModerationResult
            {
                IsApproved = body.IsSafe,
                RejectionReason = body.IsSafe ? null : body.Message,
                ReasonCode = body.Reason
            };
        }

        private static string GetContentType(string path)
            => Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

        // Matches the JSON returned by the Python API: { isSafe, reason, message }
        private sealed record AiApiResponse(bool IsSafe, string? Reason, string? Message);
    }
}
