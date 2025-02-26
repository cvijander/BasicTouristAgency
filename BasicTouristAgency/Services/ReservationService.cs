using BasicTouristAgency.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BasicTouristAgency.Services
{
    public class ReservationService : IReservationService
    {
        private readonly BasicTouristAgenctDbContext _dbContext;

        public ReservationService(BasicTouristAgenctDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AlredyUserReservedThisVacation(string userId, int vacationId)
        {
            return await _dbContext.Reservations
                .AnyAsync(r => r.UserId == userId && r.VacationId == vacationId);
        }

        public async Task<bool> CreateReservation(Reservation reservation)
        {
            if (await AlredyUserReservedThisVacation(reservation.UserId, reservation.VacationId))
            {
                
                return false;
            }
            _dbContext.Reservations.Add(reservation);

            return true;
        }

        public void DeleteReservation(int id)
        {
            Reservation reservation = _dbContext.Reservations.Find(id);

            if(reservation != null)
            {
                _dbContext.Reservations.Remove(reservation);
            }
        }

        public IEnumerable<Reservation> GetAllReservation()
        {
            return _dbContext.Reservations
                .Include(r => r.User)
                .Include(r => r.Vacation)
                .ToList();
        }

        public Reservation GetReservationById(int id)
        {
            return _dbContext.Reservations
                 .Include(r => r.User)
                 .Include(r => r.Vacation)
                 .FirstOrDefault(r => r.ReservationId == id);
        }

        public Reservation GetReservationByVacationId(int vacationId)
        {
            return _dbContext.Reservations
                   .Include(r => r.Vacation)
                   .FirstOrDefault(r => r.VacationId == vacationId);
        }

        public Reservation GetReservationByUserId(string userId)
        {
            return _dbContext.Reservations
                   .Include(r => r.User)
                   .FirstOrDefault(r => r.UserId == userId);
                   
        }

        public IEnumerable<Reservation> GetFilteredMyReservations(string userid,string vacationName, string status)
        {
            var reservations = _dbContext.Reservations
                .Include(r=> r.Vacation)
                .Where(r=> r.UserId == userid).ToList();

            if(!string.IsNullOrEmpty(vacationName))
            {
                vacationName = vacationName.ToLower().Trim();
                reservations = reservations.Where(r => r.Vacation.VacationName.ToLower().Trim().Contains(vacationName)).ToList();
            }

            if(!string.IsNullOrEmpty(status))
            {
                Reservation.ReservationStatus statusEnum;
                if(Enum.TryParse(status, true, out statusEnum))
                {
                    reservations = reservations.Where(r => r.Status == statusEnum).ToList();
                }       
                            
                
            }
            return reservations;
        }

        public void UpdateReservation(Reservation reservation)
        {
            _dbContext.Reservations.Update(reservation);
        }
    }
}
