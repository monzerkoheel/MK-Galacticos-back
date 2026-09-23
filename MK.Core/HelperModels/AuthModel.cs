namespace MK.Core.HelperModels
{
    public class AuthModel
    {
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiration { get; set; }
        public string RefreshToken { get; set; } = null!;
    }
}
