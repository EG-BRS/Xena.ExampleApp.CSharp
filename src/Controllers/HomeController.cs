using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ExampleProject.Controllers
{
    public class HomeController : Controller
    {
        private string _authority;

        public HomeController(IConfiguration configuration)
        {
            var xenaProviderSettingsSection = configuration.GetSection("XenaProvider");
            _authority = xenaProviderSettingsSection["Authority"];


        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult DiscoveryEndpoints()
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(_authority + "/.well-known/openid-configuration").Result;
                if (response.IsSuccessStatusCode)
                {
                    ViewBag.WellknownConfiguration = response.Content.ReadAsStringAsync().Result.ToPrettyJson();
                }

                response = client.GetAsync(_authority + "/.well-known/openid-configuration/jwks").Result;
                if (response.IsSuccessStatusCode)
                {
                    ViewBag.JsonWebKeys = response.Content.ReadAsStringAsync().Result.ToPrettyJson();
                }
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> TokenInformation()
        {
            var token = await HttpContext.Authentication.GetTokenAsync("access_token");

            ViewBag.Token = token;
            ViewBag.Header = token.Split('.')[0].ToPrettyJsonFromBase64();
            ViewBag.Payload = token.Split('.')[1].ToPrettyJsonFromBase64();
            ViewBag.Signature = token.Split('.')[2];
            return View();
        }

        [Authorize]
        public async Task<IActionResult> ApiExample()
        {
            var accessToken = await HttpContext.Authentication.GetTokenAsync("access_token");

            var client = new HttpClient();
            client.SetBearerToken(accessToken);
            var content = await client.GetStringAsync("https://my.xena.biz/Api/User/FiscalSetup?forceNoPaging=true");
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
