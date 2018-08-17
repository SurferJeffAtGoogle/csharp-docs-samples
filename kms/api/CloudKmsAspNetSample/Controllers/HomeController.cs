using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CloudKmsAspNetSample.Models;
using Microsoft.Extensions.Options;

namespace CloudKmsAspNetSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<SecretsModel> secrets;

        public HomeController(IOptions<SecretsModel> secrets)
        {
            this.secrets = secrets;
        }

        public IActionResult Index()
        {
            return View(secrets.Value);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
