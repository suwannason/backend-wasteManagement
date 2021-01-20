
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using backend.response;

namespace backend.Controllers
{
    [Route("fae-part/[controller]")]
    [ApiController]

    public class serviceController : ControllerBase
    {

        private readonly string GLOBAL_API_ENDPOINT;
        public serviceController(IEndpoint setting)
        {
            GLOBAL_API_ENDPOINT = setting.global_api;
        }
        [HttpGet("dept")]
        public async Task<DeptResponse> getDepDataAsync()
        {

            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                HttpResponseMessage response = client.GetAsync(GLOBAL_API_ENDPOINT + "/middleware/oracle/departments").Result;
                response.EnsureSuccessStatusCode();
                DeptResponse res = JsonConvert.DeserializeObject<DeptResponse>(await response.Content.ReadAsStringAsync());
                return res;
            }
        }
    }
}