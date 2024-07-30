using HogentVmPortal.Shared;
using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Model;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace HogentVmPortal.Services
{
    public class VirtualMachineApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public VirtualMachineApiService(IHttpClientFactory httpClientFactory, IOptions<WebApiConfig> webApiConfigOptions)
        {
            _httpClientFactory = httpClientFactory;
            var webApiConfig = webApiConfigOptions.Value;
            _apiUrl = webApiConfig.Uri;
        }

        public async Task<string> CreateVmAsync(VirtualMachineCreateRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_apiUrl}/virtualmachine/create", content);

            response.EnsureSuccessStatusCode();
            var jsonSuccessResponse = await response.Content.ReadAsStringAsync();

            return jsonSuccessResponse;
        }

        public async Task<string> RemoveVmAsync(VirtualMachineRemoveRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{_apiUrl}/virtualmachine/delete", content);

            response.EnsureSuccessStatusCode();
            var jsonSuccessResponse = await response.Content.ReadAsStringAsync();

            return jsonSuccessResponse;
        }

        public async Task<List<VirtualMachineDTO>> GetAll(bool includeUsers = false)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"{_apiUrl}/virtualmachine/all?includeUsers={includeUsers}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var vms = JsonConvert.DeserializeObject<List<VirtualMachineDTO>>(jsonResponse);

            return (vms != null) ? vms : new List<VirtualMachineDTO>();
        }

        public async Task<VirtualMachineDTO> GetById(Guid id, bool includeUsers = false)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"{_apiUrl}/virtualmachine/id?id={id}&includeUsers={includeUsers}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var vm = JsonConvert.DeserializeObject<VirtualMachineDTO>(jsonResponse);

            if (vm == null) throw new VirtualMachineNotFoundException(id.ToString());
            return vm;
        }
    }
}
