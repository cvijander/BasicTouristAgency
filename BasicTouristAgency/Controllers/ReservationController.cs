using Microsoft.AspNetCore.Mvc;

namespace BasicTouristAgency.Controllers
{
    public class ReservationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
