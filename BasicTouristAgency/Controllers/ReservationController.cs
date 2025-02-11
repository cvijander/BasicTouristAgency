using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasicTouristAgency.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult MakeAReservation()
        {
            
            var userid = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
            if(userid == null)
            {
                throw new Exception("User not authorized");
            }
            return View();
        }
    }
}
