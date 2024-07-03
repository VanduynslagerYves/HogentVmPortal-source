using HogentVmPortal.Shared.DTO;
using Newtonsoft.Json;
using System.Text;

namespace HogentVmPortal.Services
{
    public class VirtualMachineApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public VirtualMachineApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> CreateVmAsync(VirtualMachineCreateRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //TODO: get the correct url from appsettings
            //Maybe use gRPC?
            var response = await httpClient.PostAsync("https://localhost:7296/api/virtualmachine/create", content);
            //http://localhost:5067
            //https://localhost:7296
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public async Task<string> RemoveVmAsync(VirtualMachineRemoveRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //TODO: get the correct url from appsettings
            var response = await httpClient.PostAsync("https://localhost:7296/api/virtualmachine/delete", content);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return jsonResponse;
        }

        public async Task<List<VirtualMachineDTO>> GetAll(bool includeUsers = false)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.GetAsync($"https://localhost:7296/api/virtualmachine/all?includeUsers={includeUsers}");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var vms = JsonConvert.DeserializeObject<List<VirtualMachineDTO>>(jsonResponse);

                return (vms != null) ? vms : new List<VirtualMachineDTO>();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
