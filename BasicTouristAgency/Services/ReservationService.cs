using BasicTouristAgency.Models;
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

        public void CreateReservation(Reservation reservation)
        {
            _dbContext.Reservations.Add(reservation);
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

        public void UpdateReservation(Reservation reservation)
        {
            _dbContext.Reservations.Update(reservation);
        }
    }
}
