using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hello.Models;
using Microsoft.Extensions.Options;

namespace Hello.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<GreetingOptions> options;

        public HomeController(IOptions<GreetingOptions> options)
        {
            this.options = options;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(HomeIndexViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Name)) {
                model.Greeting = options.Value.Greeting;
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
