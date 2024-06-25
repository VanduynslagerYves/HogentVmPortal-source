using HogentVmPortal.Shared.DTO;
using System.Text;
using System.Text.Json;

namespace HogentVmPortal.Services
{
    public class VmApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public VmApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> CreateVmAsync(VirtualMachineCreateRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //TODO: get the correct url from appsettings
            var response = await httpClient.PostAsync("https://localhost:7296/api/virtualmachine/createvm", content);
            //http://localhost:5067
            //https://localhost:7296
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task<string> DeleteVmAsync(VirtualMachineRemoveRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //TODO: get the correct url from appsettings
            var response = await httpClient.PostAsync("https://localhost:7296/api/virtualmachine/deletevm", content);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
}
