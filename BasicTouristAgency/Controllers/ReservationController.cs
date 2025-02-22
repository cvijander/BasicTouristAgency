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

            var reservations = _unitOfWork.ReservationService.GetAllReservation();

            Console.WriteLine($"Total Reservations Before Filtering: {reservations.Count()}");

            if (!string.IsNullOrEmpty(searchUser))
            {
               reservations = reservations.Where(r => r.User.FirstName.ToLower().Trim().Contains(searchUser.ToLower().Trim()));
                
            }

            if (!string.IsNullOrEmpty(vacName))
            {
               
                reservations = reservations.Where(r => r.Vacation.VacationName.ToLower().Trim().Contains(vacName.ToLower().Trim()));
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse(status, out ReservationStatus parsedStatus))
            {
                reservations = reservations.Where(r => r.Status == parsedStatus);
            }

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
                ViewBag.ErrorMessage = "Vacation must be selected";
                return View(reservation);
            }


            var user = await _userManager.FindByIdAsync(reservation.UserId);
            if (user == null)
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
                return View("Create", reservation);
            }

            bool canReserve = await _unitOfWork.ReservationService.CanUserReserveVacation(reservation.UserId, reservation.VacationId);

            if (!canReserve)
            {
                Console.WriteLine("User already has a reservation! Showing error message...");
                ViewBag.ErrorMessage = "You have already booked this vacation";
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
                ViewBag.ErrorMessage = "An error occured while creating reservation";
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

        [HttpPost]
        [Authorize(Roles = "Admin,Tourist")]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(Reservation reservation)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "There wa an error confirmig yopur reservation";
                return View("Confirm", reservation);
            }
            _unitOfWork.ReservationService.CreateReservation(reservation);
            _unitOfWork.SaveChanges();

            ViewData["Message"] = "Reservation succesfulu confirmed";
            return RedirectToAction("MyReservation", new { userId = reservation.UserId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if(id == 0)
            {
                return NotFound();
            }

            var reservation = _unitOfWork.ReservationService.GetReservationById(id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("ReservationId, Status")] Reservation reservation )
        {
            if(id != reservation.ReservationId)
            {
                Console.WriteLine("ID mismatch - request failed.");
                return NotFound();
            }

            var existingReservation = _unitOfWork.ReservationService.GetReservationById(id);
            if(existingReservation == null )
            {
                Console.WriteLine("Reseration not found in database");
                return NotFound();
            }

            Console.WriteLine($"Before Update - Current Status: {existingReservation.Status}");
            existingReservation.Status = reservation.Status;
            Console.WriteLine($"After Update - New Status: {existingReservation.Status}");

            try
            {
                _unitOfWork.ReservationService.UpdateReservation(existingReservation);
                _unitOfWork.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                ModelState.AddModelError("", "Error updating reservation.");
            }

            return View(reservation);
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public IActionResult Delete(int id)
        {
            Reservation reservation = _unitOfWork.ReservationService.GetReservationById(id);
            if (reservation == null)
            {
                return NotFound();
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
                Reservation reservation = _unitOfWork.ReservationService.GetReservationById(id);
                if (reservation == null)
                {
                    return NotFound();
                }

                _unitOfWork.ReservationService.DeleteReservation(id);
                _unitOfWork.SaveChanges();

                return RedirectToAction("Index", new { message = "Reservation succesfuli deleted" });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occured while deleting the reservation";
                return RedirectToAction("Index", new { message = "Failed to delete reservation" });

            }
        }

        [HttpGet]
        [Authorize]
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
