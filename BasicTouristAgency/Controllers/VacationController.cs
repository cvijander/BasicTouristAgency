using BasicTouristAgency.Models;
using BasicTouristAgency.Services;
using BasicTouristAgency.Util.Helpers;
using BasicTouristAgency.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.AccessControl;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BasicTouristAgency.Controllers
{
    public class VacationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;

               

        public VacationController(IUnitOfWork unitOfWork, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
        }


        [HttpGet]
        public IActionResult Index(string message,int? minPrice, int? maxPrice, string vacationName, DateTime? startDate, DateTime? endDate, Vacation.VacationType? vacType, string sortBy = "StartDate", bool descending = false, int page = 1)
        {
            ViewBag.Message = message;
            ViewBag.VacationTypes = VacationUtils.GetVacationTypes();

            var vacations = _unitOfWork.VacationService.GetAllFilteredVacation(minPrice, maxPrice, vacationName, startDate, endDate, vacType, sortBy, descending);




           PaginationViewModel<Vacation> pgvm = new  ViewModel.PaginationViewModel<Vacation>();            
            
           pgvm.TotalCount = vacations.Count();
           pgvm.CurrentPage = page;
           pgvm.PageSize = 5;

           vacations = vacations.Skip(pgvm.PageSize * (pgvm.CurrentPage - 1)).Take(pgvm.PageSize).ToList();
           pgvm.Collection = vacations;

            return View(pgvm);
        }


        [Authorize]
        [Authorize(Roles ="Tourist")]
        public IActionResult Details(int id)
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.VacationTypes = VacationUtils.GetVacationTypes();          

            
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [AutoValidateAntiforgeryToken]
        public IActionResult Create(Vacation vacation)
        { 
            
            if(!ModelState.IsValid)
            {
                ViewBag.VacationTypes = VacationUtils.GetVacationTypes();
                return View("Create",vacation);
            }

            try
            {
                _unitOfWork.VacationService.CreateVacation(vacation);
                _unitOfWork.SaveChanges();

                TempData["Success"] = "Vacation succesfulu created. ";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the vacation.";
                _emailSender.SendEmailAsync("cvija85@gmail.com", "There is an error", "An error occured while creating vacation");
                return View("Create", vacation);
            }                   
            
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Console.WriteLine("================ Debbugind user identity =================");
            Console.WriteLine($"User: {User.Identity.Name}");
            Console.WriteLine($"Is Authenticated: {User.Identity.IsAuthenticated}");
            Console.WriteLine($"Roles: {string.Join(", ", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value))}");
            Console.WriteLine("==============================================");

            if (id <= 0)
            {
                TempData["Error"] = "Vacation Id not found.";
                return RedirectToAction("NotFound","Home");
            }

            Vacation vacation = _unitOfWork.VacationService.GetVacationById(id);

            if(vacation == null)
            {
                TempData["Error"] = "Vacation not found.";
                _emailSender.SendEmailAsync("cvija85@gmail.com", "There is an error not found", "Not found");
                return RedirectToAction ("NotFound","Home");
            }


            ViewBag.VacationTypes = VacationUtils.GetVacationTypes();
            return View(vacation);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Vacation vacation)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.VacationTypes = VacationUtils.GetVacationTypes();
                 
                return View("Edit", vacation);
            }

            try
            {
                bool wasConvertedToLastMinute = false;

                if(vacation.Type != Vacation.VacationType.LastMinute && vacation.StartDate <= DateTime.Now.AddDays(10).Date)
                {
                    vacation.Type = Vacation.VacationType.LastMinute;
                    wasConvertedToLastMinute = true;
                }

                _unitOfWork.VacationService.UpdateVacation(vacation);
                _unitOfWork.SaveChanges();

                if(wasConvertedToLastMinute)
                {
                    TempData["Message"] = "Your vacatin package was converted to a Last minute";
                }
                else
                {
                    TempData["Message"] = "Vacation detials succesfully updated";
                }
               
                return RedirectToAction("Index" );

            }
            catch (Exception ex )
            {
                TempData["Error"] = "An error arrived during update vacation";

                ViewBag.VacationTypes = VacationUtils.GetVacationTypes();

                return View("Edit", vacation);
                
            }
            
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Delete (int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "vacation id not found";
                return RedirectToAction("NotFound", "Home");
            }

            Vacation vacation = _unitOfWork.VacationService.GetVacationById(id);
            if (vacation == null)
            {
                TempData["Error"] = "Vacation not found";
                return RedirectToAction("NotFound", "Home");
            }

            return View(vacation);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]

        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                Vacation vacation = _unitOfWork.VacationService.GetVacationById(id);
                if(vacation == null)
                {
                    return NotFound();
                }

                _unitOfWork.VacationService.DeleteVacation(id);
                _unitOfWork.SaveChanges();

                return RedirectToAction("Index" , new { message = "Vacation succesfuli deleted"});
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occured while deleting the vacation";
                return RedirectToAction("Index", new { message = "Failed to delete vacation" });
                
            }
        }

    }

        
}
