using System.Text.Json.Serialization;

namespace MK.API.Errors
{
    public class ApiResponse
    {
        [JsonPropertyOrder(1)]
        public int StatusCode { get; set; }
        [JsonPropertyOrder(2)]
        public string? Message { get; set; }
        public ApiResponse(int SD, string? M = null)
        {
            StatusCode = SD;
            if (M != null)
            { Message = M; }
            else { Message = GetDefaultMessageForStatusCode(SD); }
        }
        private string? GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "UnAuthorized",
                404 => "Not Found",
                403 => "Forbidden",
                500 => "Server Error",
                _ => null
            };
        }
    }
}
