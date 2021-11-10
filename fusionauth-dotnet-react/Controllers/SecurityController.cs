using fusionauth_dotnet_react.Helpers;
using fusionauth_dotnet_react.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fusionauth_dotnet_react.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly IOptions<FusionAuth> _fusionAuthOptions;
        private readonly HttpClient _httpClient;

        public SecurityController(IOptions<FusionAuth> fusionAuthOptions, HttpClient httpClient)
        {
            _fusionAuthOptions = fusionAuthOptions;
            _httpClient = httpClient;
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            var verifier = Pkce.GenerateCodeVerifier();
            Console.WriteLine($"Verifier: {verifier}");
            HttpContext.Session.SetString(SessionKeys.Verifier, verifier);

            var challenge = Pkce.GenerateChallenge(verifier);
            Console.WriteLine($"Challenge: {challenge}");

            var nonce = Pkce.GenerateNonce();
            HttpContext.Session.SetString(SessionKeys.Nonce, nonce);

            return Redirect($"{_fusionAuthOptions.Value.Authority}/oauth2/authorize?client_id={_fusionAuthOptions.Value.ClientId}&redirect_uri={_fusionAuthOptions.Value.RedirectUri}&response_type=code&code_challenge={challenge}&code_challenge_method=S256&nonce={nonce}&scope=openid offline_access");
        }

        [HttpGet]
        [Route("Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Redirect($"{_fusionAuthOptions.Value.Authority}/oauth2/logout?client_id={_fusionAuthOptions.Value.ClientId}");
        }

        [HttpGet]
        [Route("oauth-callback")]
        public async Task<IActionResult> OAuth_CallBack([FromQuery] OAuthCallBack oAuthCallBack)
        {
            var verifier = HttpContext.Session.GetString(SessionKeys.Verifier);
            var formData = new Dictionary<string, string>
            {
                { "client_id", _fusionAuthOptions.Value.ClientId },
                { "client_secret", _fusionAuthOptions.Value.ClientSecret },
                { "code", oAuthCallBack.Code },
                { "code_verifier", verifier },
                { "grant_type", "authorization_code" },
                { "redirect_uri", _fusionAuthOptions.Value.RedirectUri },
            };
            var content = new FormUrlEncodedContent(formData);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            var result = await _httpClient.PostAsync($"{_fusionAuthOptions.Value.Authority}/oauth2/token", content);

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response);

                HttpContext.Session.SetString(SessionKeys.Token, tokenResponse.access_token);
                HttpContext.Session.SetString(SessionKeys.RefreshToken, tokenResponse.refresh_token);

                return Redirect($"{Request.Scheme}://{Request.Host}{Request.PathBase}");
            }

            return BadRequest();
        }

        public class OAuthCallBack
        {
            public string Code { get; set; }
        }

        public class TokenRequest
        {
            public string client_id { get; set; }

            public string client_secret { get; set; }

            public string code { get; set; }

            public string code_verifier { get; set; }

            public string grant_type { get; set; }

            public string redirect_uri { get; set; }
        }

        public class TokenResponse
        {
            public string access_token { get; set; }

            public int expires_in { get; set; }

            public string refresh_token { get; set; }
        }
    }
}
