using System.Text.Json;

namespace AuthService.Services {
    public class EmailValidator : IEmailValidator {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<bool> IsValid(string email, string apiKey) {
            try {
                var url = $"https://api.hunter.io/v2/email-verifier?email={Uri.EscapeDataString(email)}&api_key={apiKey}";
                using var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return false;

                using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("data", out var dataElement) && dataElement.TryGetProperty("status", out var statusElement))
                    return statusElement.GetString() == "valid";

                return false;
            }
            catch { return false; }
        }
    }
}