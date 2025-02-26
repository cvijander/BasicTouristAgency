using BasicTouristAgency.Models;
using BasicTouristAgency.Services;
using BasicTouristAgency.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using static BasicTouristAgency.Models.Reservation;

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

        [HttpGet]
        [Authorize(Roles ="Admin")]

        public IActionResult Index(string searchUser,string vacName,string status, int page = 1)
        {

            var reservations = _unitOfWork.ReservationService.GetFilteredAllReservations(searchUser,vacName,status);
            

            PaginationViewModel<Reservation> pgvmReservation = new ViewModel.PaginationViewModel<Reservation>();

            pgvmReservation.TotalCount = reservations.Count();
            pgvmReservation.CurrentPage =page;
            pgvmReservation.PageSize = 5;

            reservations = reservations.Skip(pgvmReservation.PageSize * (pgvmReservation.CurrentPage -1)).Take(pgvmReservation.PageSize).ToList();
            pgvmReservation.Collection = reservations;
                                    
            return View(pgvmReservation);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Tourist")]

        public async Task<IActionResult> Create(int vacationId)
        {
            if (vacationId <= 0)
            {
                TempData["Error"] = "Vacation id does not exists";
                return RedirectToAction("NotFound", "Home");
            }

            Vacation vacation = _unitOfWork.VacationService.GetVacationById(vacationId);

            if (vacation == null)
            {
                TempData["Error"] = "Vacation does not exists";
                return RedirectToAction("NotFound", "Home");
            }

            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userid))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userid);
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
        [Authorize(Roles = "Admin,Tourist")]

        public async Task<IActionResult> Create(Reservation reservation)
        {
            Console.WriteLine("Post test ");
            Console.WriteLine($"Vacation id {reservation.VacationId}, UserId {reservation.UserId}");
            Console.WriteLine($"Date {reservation.DateCreatedReservation} , status {reservation.Status}");


            reservation.Vacation = _unitOfWork.VacationService.GetVacationById(reservation.VacationId);


            if (reservation.Vacation == null || reservation.VacationId == 0)
            {
                TempData["Error"] = "Vacation must be selected";
                return View(reservation);
            }


            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userid))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return BadRequest("User not found in database");
            }

            reservation.User = user;

            if (DateTime.Today >= reservation.Vacation.StartDate)
            {
                 TempData["Error"]= "Reservations can only be made before the vacation begins.";
                return View(reservation);
            }

            if (reservation.DateCreatedReservation.Date < DateTime.Today)
            {
                TempData["Error"] = "The reservation cannot be in the past.";
                return View(reservation);
            }

            if(reservation.DateCreatedReservation > reservation.Vacation.StartDate)
            {
                TempData["Error"] = "The reservation can not be after the start date";
                return View(reservation);
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid!");

                foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {modelError.ErrorMessage}");
                }

                reservation.Vacation = _unitOfWork.VacationService.GetVacationById(reservation.VacationId);
                return View("Create", reservation);
            }

            bool canReserve = await _unitOfWork.ReservationService.AlredyUserReservedThisVacation(reservation.UserId, reservation.VacationId);

            if (canReserve)
            {
                Console.WriteLine("User already has a reservation! Showing error message...");
                TempData["Error"] = "You have already booked this vacation";
                return View("Create", reservation);
            }

            reservation.Status = Reservation.ReservationStatus.Created;

            try
            {
                await _unitOfWork.ReservationService.CreateReservation(reservation);
                _unitOfWork.SaveChanges();

                TempData["Message"] = "Reservation succesfuly created ";
                return RedirectToAction("Confirm", "Reservation", new { reservationId = reservation.ReservationId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occured while creating reservation";
                _emailSender.SendEmailAsync("cvija85@gmail.com", "There is an error while createing reservation", "An error occured while creating reservation");

                reservation.Vacation = _unitOfWork.VacationService.GetVacationById(reservation.VacationId);

                return View("Create", reservation);
            }


        }

        [HttpGet]
        [Authorize(Roles = "Admin,Tourist")]
        public async Task<IActionResult> Confirm(int reservationId)
        {
            Console.WriteLine("Get confirm called");
            Console.WriteLine($"ReservationId {reservationId}");

            if (reservationId <= 0)
            {
                TempData["Error"] = "Reservation id not found";
                return RedirectToAction("NotFound", "Home");
            }

            Reservation reservation = _unitOfWork.ReservationService.GetReservationById(reservationId);

            if (reservation == null)
            {
                Console.WriteLine("reservation not found");
                TempData["Error"] = "reservvation not found";
                return RedirectToAction("Index", "Vacation");
            }

            var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userid))
            {
                return Unauthorized();
            }

            if (reservation.UserId != userid)
            {
                Console.WriteLine("Unauthorized access attempt to a reservation!");
                TempData["Error"] = "You are not authorized to view this reservation.";
                return RedirectToAction("Index", "Vacation");
            }

            string subject = "Reservation Confirmation";
            string body = $@"
                       <h2>Thank you for tour reservation</h2>
                       <p><strong>Destination:</strong> {reservation.Vacation.VacationName}</p> 
                       <p><strong>Start date </strong> {reservation.Vacation.StartDate.ToShortDateString()}</p>
                       <p><strong>Status </strong> {reservation.Status}</p>
                       <br>
                       <p>You can view you reservation gere <a href='https://localhost:7098/Reservation/MyReservations'>My reservations</a></p>";

            await _emailSender.SendEmailAsync(User.Identity.Name, subject, body);

            string adminEmail = "cvija85@gmail.com";
            string adminSubject = "New Reservation created";
            string adminBody = $@"
                       <h2>New reservation notofikacion</h2>
                       <p><strong>User </strong> {User.Identity.Name} has made a reservation</p>
                       <p><strong>Destination </strong> {reservation.Vacation.VacationName}</p>
                       <p><strong>Start date </strong> {reservation.Vacation.StartDate.ToShortDateString()}</p>
                       <p><strong>Status</strong>{reservation.Status}
                       <br>
                       <p>You can manage reservations here: <a href='https://localhost:7098/Reservation/Index'>All Reservations</a></p>";

            await _emailSender.SendEmailAsync(adminEmail, adminSubject, adminBody);
                         


            return View(reservation);


        }
                

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if(id <= 0)
            {
                TempData["Error"] = "Reservation id not found";
                return  RedirectToAction("NotFound","Home");
            }

            var reservation = _unitOfWork.ReservationService.GetReservationById(id);

            if (reservation == null)
            {
                TempData["Error"] = "Reservation  not found";
                return RedirectToAction("NotFound", "Home");
            }

            return View(reservation);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, string status )
        {
            if(id <= 0 )
            {
                TempData["Error"] = "Reservati id not found";
                return RedirectToAction("NotFound", "Home");
            }
                       

            var reservation = _unitOfWork.ReservationService.GetReservationById(id);

            if(reservation == null )
            {
                TempData["Error"] ="Reseration not found in database";
                return RedirectToAction("NotFound", "Home");
            }

            if(Enum.TryParse(status, out Reservation.ReservationStatus parsedStatus))
                {
                    reservation.Status = parsedStatus;
                }
                else
                {
                    TempData["Error"] = "Invalida reservation stratus";
                    return View(reservation);
                }
            

            try
            {
                _unitOfWork.ReservationService.UpdateReservation(reservation);
                _unitOfWork.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                TempData["Error"]= "Error updating reservation.";
                return RedirectToAction("Index", "Reservations");
            }

            
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Reservation id not found";
                return RedirectToAction("NotFound", "Home");
            }

            Reservation reservation = _unitOfWork.ReservationService.GetReservationById(id);
            if (reservation == null)
            {
                TempData["Error"] = "Reservation  not found";
                return RedirectToAction("NotFound", "Home");
            }

            return View(reservation);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]

        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                if(id <= 0)
                {
                    TempData["Error"] = "Rezevation id not found";
                    return RedirectToAction("NotFound", "Home");
                }

                Reservation reservation = _unitOfWork.ReservationService.GetReservationById(id);
                if (reservation == null)
                {
                    TempData["Error"] = "Rezevation  not found";
                    return RedirectToAction("NotFound", "Home");
                }

                _unitOfWork.ReservationService.DeleteReservation(id);
                _unitOfWork.SaveChanges();

                TempData["Success"] = "Reservation succesfuli deleted";
                return RedirectToAction("Index", "Reservation");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occured while deleting the reservation";
                return RedirectToAction("Index", "Reservation");

            }
        }

        [HttpGet]
        [Authorize(Roles ="Admin,Tourist")]
        public async Task<IActionResult> MyReservations(string vacationName, string status, int page =1 )
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reservations = _unitOfWork.ReservationService.GetFilteredMyReservations(currentUser.Id,vacationName, status);

            PaginationViewModel<Reservation> pgvmMyReservation = new ViewModel.PaginationViewModel<Reservation>();

            pgvmMyReservation.TotalCount = reservations.Count();
            pgvmMyReservation.CurrentPage = page;
            pgvmMyReservation.PageSize = 5;

            reservations = reservations.Skip(pgvmMyReservation.PageSize * (pgvmMyReservation.CurrentPage - 1)).Take(pgvmMyReservation.PageSize).ToList();
            pgvmMyReservation.Collection = reservations;


            return View(pgvmMyReservation);           
        }

    }
}
