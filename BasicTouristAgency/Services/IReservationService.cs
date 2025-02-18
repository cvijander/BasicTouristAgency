using BasicTouristAgency.Models;

namespace BasicTouristAgency.Services
{
    public interface IReservationService
    {
        IEnumerable<Reservation> GetAllReservation();

        Reservation GetReservationById(int id);

        void CreateReservation (Reservation reservation);

        void UpdateReservation (Reservation reservation);

        void DeleteReservation (int id);

        Reservation GetReservationByVacationId(int vacationId);
    }
}
