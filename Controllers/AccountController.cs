using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace MvcClient.Controllers
{
    public class AccountController : Controller
    {
        public async Task Login()
        {
            await HttpContext.ChallengeAsync("Authentiq", new AuthenticationProperties() { RedirectUri = "/Home/Secure" });
        }

        [Authorize]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("Authentiq");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
