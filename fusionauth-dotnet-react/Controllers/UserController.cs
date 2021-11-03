using fusionauth_dotnet_react.Helpers;
using fusionauth_dotnet_react.Models.User;
using fusionauth_dotnet_react.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fusionauth_dotnet_react.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IOptions<FusionAuth> _fusionAuthOptions;
        private readonly HttpClient _httpClient;

        public UserController(IOptions<FusionAuth> fusionAuthOptions, HttpClient httpClient)
        {
            _fusionAuthOptions = fusionAuthOptions;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var token = HttpContext.Session.GetString(SessionKeys.Token);
            if (string.IsNullOrEmpty(token))
            {
                return Ok();
            }

            var claims = User.Identities.First().Claims.ToList();
            var userId = claims?.FirstOrDefault(x => x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_fusionAuthOptions.Value.ApiKey);
            var response = await _httpClient.GetStringAsync($"{_fusionAuthOptions.Value.Authority}/api/user/registration/{userId}/{_fusionAuthOptions.Value.ClientId}");
            return Ok(new
            {
                Registration = JsonDocument.Parse(response).RootElement.GetProperty("registration"),
                Token = token,
            });
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PostRequest requestData)
        {
            var token = HttpContext.Session.GetString(SessionKeys.Token);
            if (string.IsNullOrEmpty(token))
            {
                return Ok();
            }

            var claims = User.Identities.First().Claims.ToList();
            var userId = Guid.Parse(claims?.FirstOrDefault(x => x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_fusionAuthOptions.Value.ApiKey);

            var json = "{ \"registration\":{ \"data\":{ \"userData\":\"" + requestData.UserData + "\"} } }";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync(
                $"{_fusionAuthOptions.Value.Authority}/api/user/registration/{userId}/{_fusionAuthOptions.Value.ClientId}",
                content);

            return Ok(response);
        }
    }
}
