

using System.Text.Json.Serialization;

namespace Toyo.Blockchain.Domain.Dtos.Authentication
{
    public class JwtAuthenticationDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in_minutes")]
        public int ExpiresInMinutes { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }
}