using System.Net.Http.Json;
using CustomerService.Models.DTOs;

namespace CustomerService.Services.Http
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> GetUserEmailAsync(string userId)
        {
            var response = await _httpClient.GetFromJsonAsync<UserEmailDto>(
                $"/api/users/{userId}/email");

            return response?.Email;
        }
    }
}
