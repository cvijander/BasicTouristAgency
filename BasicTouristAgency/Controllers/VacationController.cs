using BasicTouristAgency.Services;
using Microsoft.AspNetCore.Mvc;

namespace BasicTouristAgency.Controllers
{
    public class VacationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VacationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            var vacations = _unitOfWork.VacationService.GetAllVacations();
            return View(vacations);
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }
    }

        
}
