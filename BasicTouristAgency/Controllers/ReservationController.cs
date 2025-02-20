using BasicTouristAgency.Models;
using BasicTouristAgency.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasicTouristAgency.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<User> _userManager;

        public ReservationController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]

        public async Task<IActionResult> Create(int vacationId)
        {
            Vacation vacation = _unitOfWork.VacationService.GetVacationById(vacationId);

            if (vacation == null)
            {
                return NotFound();
            }

            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userid))
            {
                return Unauthorized();
            }

            var user = await  _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return BadRequest("User not found in database");
            }

            var reservation = new Reservation
            {
                UserId = userid,
                User = user,
                VacationId = vacationId,
                Vacation = vacation,
                DateCreatedReservation = DateTime.Today,
                Status = Reservation.ReservationStatus.Created
            };

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]

        public async Task<IActionResult> Create(Reservation reservation)
        {
            Console.WriteLine("Post test ");
            Console.WriteLine($"Vacation id {reservation.VacationId}, UserId {reservation.UserId}");
            Console.WriteLine($"Date {reservation.DateCreatedReservation} , status {reservation.Status}");


            reservation.Vacation = _unitOfWork.VacationService.GetVacationById(reservation.VacationId);
            

            if (reservation.Vacation == null || reservation.VacationId == 0)
            {
                ViewBag.ErrorMessage = "Vacation must be selected";
                return View(reservation);
            }


            var user = await _userManager.FindByIdAsync(reservation.UserId);
            if (user  == null)
            {
                ViewBag.ErrorMessage = "User not found";
                return View(reservation);
            }

            reservation.User = user;

            if (DateTime.Today >= reservation.Vacation.StartDate)
            {
                ModelState.AddModelError(nameof(reservation.DateCreatedReservation), "Reservations can only be made before the vacation begins.");
            }

            if (reservation.DateCreatedReservation.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(reservation.DateCreatedReservation), "The reservation cannot be in the past.");
            }


            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid!");

                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {modelError.ErrorMessage}");
                }

                reservation.Vacation = _unitOfWork.VacationService.GetVacationById(reservation.VacationId);
                return View("Create",reservation);
            }
                    
            bool canReserve = await _unitOfWork.ReservationService.CanUserReserveVacation(reservation.UserId, reservation.VacationId);
                     
            if(!canReserve)
            {
                Console.WriteLine("User already has a reservation! Showing error message...");
                ViewBag.ErrorMessage = "You have already booked this vacation";
                return View("Create",reservation);
            }
            
            reservation.Status = Reservation.ReservationStatus.Created;

            try
            {
               await _unitOfWork.ReservationService.CreateReservation(reservation);
                _unitOfWork.SaveChanges();

                TempData["Message"] = "Reservation succesfuly created ";
                return RedirectToAction("Confirm", "Reservation", new { reservationId = reservation.ReservationId});
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occured while creating reservation";
                _emailSender.SendEmailAsync("cvija85@gmail.com", "There is an error while createing reservation", "An error occured while creating reservation");

                reservation.Vacation = _unitOfWork.VacationService.GetVacationById(reservation.VacationId);

                return View("Create",reservation);
            }


        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public IActionResult Confirm(int reservationId)
        {
            Console.WriteLine("Get confirm called");
            Console.WriteLine($"ReservationId {reservationId}");


            Reservation reservation = _unitOfWork.ReservationService.GetReservationById(reservationId);

            if (reservation == null)
            {
                Console.WriteLine("reservation not found");
                TempData["Error"] = "reservvation not found";
                return RedirectToAction("Index","Vacation");
            }

            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userid))
            {
                return Unauthorized();
            }

            if( reservation.UserId != userid)
            {
                Console.WriteLine("Unauthorized access attempt to a reservation!");
                TempData["Error"] = "You are not authorized to view this reservation.";
                return RedirectToAction("Index", "Vacation");
            }
                        

            return View(reservation);

            
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(Reservation reservation)
        {
            if(!ModelState.IsValid)
            {
                ViewData["Error"] = "There wa an error confirmig yopur reservation";
                return View("Confirm", reservation);
            }
            _unitOfWork.ReservationService.CreateReservation(reservation);
            _unitOfWork.SaveChanges();

            ViewData["Message"] = "Reservation succesfulu confirmed";
            return RedirectToAction("MyReservation", new { userId = reservation.UserId});
        }

    }
}
