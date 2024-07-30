using HogentVmPortal.Shared.Model;
using Newtonsoft.Json;
using System.Text;

namespace HogentVmPortal.RequestQueue.CtHandler.Services
{
    //TODO: move this to VmPortal for use in the webapp to retreive status
    public class RequestApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RequestApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        //public async Task<string> Update(Request request)
        //{
        //    var httpClient = _httpClientFactory.CreateClient();
        //    var jsonContent = JsonConvert.SerializeObject(request, Formatting.Indented);
        //    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        //    var response = await httpClient.PostAsync($"https://localhost:7296/api/request/update", content);
        //    response.EnsureSuccessStatusCode();

        //    var jsonResponse = await response.Content.ReadAsStringAsync();

        //    var jsonSuccessResponse = await response.Content.ReadAsStringAsync();

        //    return jsonSuccessResponse;
        //}

        //public async Task<Request> GetById(Guid id)
        //{
        //    var httpClient = _httpClientFactory.CreateClient();

        //    var response = await httpClient.GetAsync($"https://localhost:7296/api/request/id?id={id}");
        //    response.EnsureSuccessStatusCode();

        //    var jsonResponse = await response.Content.ReadAsStringAsync();
        //    var req = JsonConvert.DeserializeObject<Request>(jsonResponse);

        //    if (req == null) throw new RequestNotFoundException(id.ToString());
        //    return req;
        //}
    }
}
