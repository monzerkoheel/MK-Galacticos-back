using System.Text.Json.Serialization;

namespace MK.API.Errors
{
    public class ApiExceprtionErrorRespons : ApiResponse
    {
        [JsonPropertyOrder(3)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Detsils { get; set; }
        public ApiExceprtionErrorRespons(int SD, string? M = null, string? D = null) : base(SD, M)
        {
            Detsils = D;
        }

    }
}
