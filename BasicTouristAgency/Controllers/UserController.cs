using Microsoft.AspNetCore.Mvc;

namespace BasicTouristAgency.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
