using Newtonsoft.Json;

namespace HogentVmPortal.Services
{
    public class ValidateApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ValidateApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ValidateName(string name)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"https://localhost:7296/api/validate/name?name={name}");
            response.EnsureSuccessStatusCode();

            var jsonSuccessResponse = await response.Content.ReadAsStringAsync();
            var isValid = JsonConvert.DeserializeObject<bool>(jsonSuccessResponse);

            return isValid;
        }
    }
}
