using BasicTouristAgency.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BasicTouristAgency.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Index action has been called.");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("An error occurred.");
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("Home/Error/{statusCode}")]
        public IActionResult Error(int? statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound"); 
            }
            return View("Error");
        }

        public IActionResult NotFound()
        {
            TempData.Keep("Error");
            return View("NotFound"); 
        }

        public IActionResult TestNotFound()
        {
            return View("NotFound");
        }
    }
}
