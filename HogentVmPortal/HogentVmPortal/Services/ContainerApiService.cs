﻿using HogentVmPortal.Shared.DTO;
using Newtonsoft.Json;
using System.Text;

namespace HogentVmPortal.Services
{
    public class ContainerApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ContainerApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> Validate(ContainerCreateRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //TODO: get the correct url from appsettings
            var response = await httpClient.PostAsync("https://localhost:7296/api/container/validate", content);

            response.EnsureSuccessStatusCode();
            var jsonSuccessResponse = await response.Content.ReadAsStringAsync();
            var isValid = JsonConvert.DeserializeObject<bool>(jsonSuccessResponse);

            return isValid;
        }

        public async Task<string> CreateContainerAsync(ContainerCreateRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://localhost:7296/api/container/create", content);

            response.EnsureSuccessStatusCode();
            var jsonSuccessResponse = await response.Content.ReadAsStringAsync();

            return jsonSuccessResponse;
        }

        public async Task<string> RemoveContainerAsync(ContainerRemoveRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            //TODO: get the correct url from appsettings
            var response = await httpClient.PostAsync("https://localhost:7296/api/container/delete", content);

            response.EnsureSuccessStatusCode();
            var jsonSuccessResponse = await response.Content.ReadAsStringAsync();

            return jsonSuccessResponse;
        }

        public async Task<List<ContainerDTO>> GetAll(bool includeUsers = false)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetAsync($"https://localhost:7296/api/container/all?includeUsers={includeUsers}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var vms = JsonConvert.DeserializeObject<List<ContainerDTO>>(jsonResponse);

            return (vms != null) ? vms : new List<ContainerDTO>();
        }
    }
}
