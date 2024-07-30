using HogentVmPortal.Shared.DTO;
using HogentVmPortal.Shared.Model;
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

        //TODO: save the request as a virtualmachine and status.
        // validate request: check vm's for existing name
        public async Task<string> CreateVmAsync(VirtualMachineCreateRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //TODO: get the correct url from appsettings
            var response = await httpClient.PostAsync("https://localhost:7296/api/virtualmachine/create", content);

            response.EnsureSuccessStatusCode();
            var jsonSuccessResponse = await response.Content.ReadAsStringAsync();

            return jsonSuccessResponse;
        }

        public async Task<string> RemoveVmAsync(VirtualMachineRemoveRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //TODO: get the correct url from appsettings
            var response = await httpClient.PostAsync("https://localhost:7296/api/virtualmachine/delete", content);

            response.EnsureSuccessStatusCode();
            var jsonSuccessResponse = await response.Content.ReadAsStringAsync();

            return jsonSuccessResponse;
        }

        public async Task<List<VirtualMachineDTO>> GetAll(bool includeUsers = false)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"https://localhost:7296/api/virtualmachine/all?includeUsers={includeUsers}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var vms = JsonConvert.DeserializeObject<List<VirtualMachineDTO>>(jsonResponse);

            return (vms != null) ? vms : new List<VirtualMachineDTO>();
        }

        public async Task<VirtualMachineDTO> GetById(Guid id, bool includeUsers = false)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"https://localhost:7296/api/virtualmachine/id?id={id}&includeUsers={includeUsers}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var vm = JsonConvert.DeserializeObject<VirtualMachineDTO>(jsonResponse);

            if (vm == null) throw new VirtualMachineNotFoundException(id.ToString());
            return vm;
        }
    }
}
