using System.Net.Http.Json;
using System.Text.Json;
 using System.Text.Json.Serialization;
namespace SurveyService.Services
{
    public class AuthClient
    {
        private readonly HttpClient _httpClient;

        public AuthClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AuthResult?> Login(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:5001/api/auth/login",
                new { username, password });

            // ❌ API lỗi
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Auth API ERROR: " + err);
                return null;
            }

            // đọc raw JSON
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Auth JSON: " + json);

            // deserialize chuẩn
            var result = JsonSerializer.Deserialize<AuthResult>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
    }
   

    public class AuthResult
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; }

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; }
    }
}