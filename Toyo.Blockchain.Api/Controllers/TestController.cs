using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Toyo.Blockchain.Api.Helpers;

namespace Toyo.Blockchain.Api.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class TestController : ControllerBase
    {
        private ILoginHelper _loginHelper;
        private HttpClient _httpClient;
        public TestController(ILoginHelper loginHelper, IHttpClientFactory factory)
        {
            _loginHelper = loginHelper;
            _httpClient = factory.CreateClient("toyoBackend");
        }

        [HttpGet]
        [Route("test")]
        public async Task<ActionResult<string>> Test()
        {
            var isValid = _loginHelper.TokenIsValid(_httpClient);
            _loginHelper.GenerateToken(_httpClient);
            var response = await _httpClient.GetStringAsync("api/login/authorization");

            return "OK";
        }
    }
}