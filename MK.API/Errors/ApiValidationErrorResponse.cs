using System.Text.Json.Serialization;

namespace MK.API.Errors
{
    public class ApiValidationErrorResponse : ApiResponse
    {
        [JsonPropertyOrder(3)]
        public ICollection<string> Errors { get; set; }
        public ApiValidationErrorResponse() : base(400)
        {
            Errors = new List<string>();
        }
    }
}
