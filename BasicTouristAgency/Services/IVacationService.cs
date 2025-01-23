using BasicTouristAgency.Models;

namespace BasicTouristAgency.Services
{
    public interface IVacationService
    {
        IEnumerable<Vacation> GetAllVacations();

        Vacation GetVacationById(int id);

        void CreateVacation (Vacation vacation);

        void UpdateVacation (Vacation vacation);

        void DeleteVacation (int id);
    }
}
