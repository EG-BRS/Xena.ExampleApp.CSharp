using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ExampleProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly XenaProviderSettings _xenaSettings;
        private HttpClient _client = new HttpClient();
        public HomeController(IOptions<XenaProviderSettings> xenaSettings)
        {
            _xenaSettings = xenaSettings.Value;

        }
        public IActionResult Index()
        {
            ViewBag.ClientID = _xenaSettings.ClientID;
            ViewBag.NotConfigured = _xenaSettings.ClientID == "";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> DiscoveryEndpoints()
        {
            //Get the well-known configration from Xena....
            var response = await _client.GetAsync(_xenaSettings.Authority + "/.well-known/openid-configuration");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.WellknownConfiguration = response.Content.ReadAsStringAsync().Result.ToPrettyJson();
            }

            //Lets get the signing certificates aswell...
            response = await _client.GetAsync(_xenaSettings.Authority + "/.well-known/openid-configuration/jwks");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.JsonWebKeys = content.ToPrettyJson();
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> TokenInformation()
        {
            //Get the token and split it into its three parts and decoded them if needed...
            var token = await HttpContext.Authentication.GetTokenAsync("access_token");
            ViewBag.Token = token;
            var parts = token.Split('.');
            ViewBag.Header = parts[0].ToPrettyJsonFromBase64();
            ViewBag.Payload = parts[1].ToPrettyJsonFromBase64();
            ViewBag.Signature = parts[2];

            return View();
        }

        [Authorize]
        public async Task<IActionResult> ApiExample()
        {
            var accessToken = await HttpContext.Authentication.GetTokenAsync("access_token");

            //Get a list of fiscals from your account in Xena...
            _client.SetBearerToken(accessToken);
            var content = await _client.GetStringAsync("https://my.xena.biz/Api/User/FiscalSetup?forceNoPaging=true");
            ViewBag.Json = JObject.Parse(content).ToString();
            return View();
        }

        public async Task Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
            await HttpContext.Authentication.SignOutAsync("oidc");
        }
    }
}
