using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using RestSharp;
using WebApp_OpenIDConnect_DotNet.Models;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GraphServiceClient _graphServiceClient;

        public HomeController(ILogger<HomeController> logger,
            GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> UserDescription()
        {
            var userInformationByName = new Dictionary<string, string>();

            var userIdentity = User.Identity;
            AddIdentity(userInformationByName, userIdentity);

            var claims = User.Claims;

            AddClaims(claims, userInformationByName);

            var identities = User.Identities;

            foreach (var identity in identities)
            {
                AddIdentity(userInformationByName, identity);
                AddClaims(identity.Claims, userInformationByName);
            }

            await AddGraphDataAsync(userInformationByName);


            return View(userInformationByName);
        }

        private async Task AddGraphDataAsync(Dictionary<string, string> userInformationByName)
        {
            var user = await _graphServiceClient.Me.Request().GetAsync();

            foreach (var field in user
                         .GetType()
                         .GetProperties()
                         .Where(f => f.GetValue(user) != null))
            {
                userInformationByName[$"Graph:{field.Name}"] = field.GetValue(user)?.ToString();
            }
        }

        private static void AddClaims(IEnumerable<Claim> claims, Dictionary<string, string> userInformationByName)
        {
            foreach (var claim in claims)
            {
                userInformationByName[claim.Type] = claim.Value;
            }
        }

        private static void AddIdentity(Dictionary<string, string> userInformationByName, IIdentity userIdentity)
        {
            userInformationByName["Name"] = userIdentity?.Name;
            userInformationByName["IsAuthenticated?"] = userIdentity?.IsAuthenticated.ToString();
            userInformationByName["Authentication type"] = userIdentity?.AuthenticationType;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
