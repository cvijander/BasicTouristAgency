using BasicTouristAgency.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasicTouristAgency.Services
{
    public interface IVacationService
    {
        IEnumerable<Vacation> GetAllVacations();

        IEnumerable<Vacation> GetAllFilteredVacation(int? minPrice, int? maxPrice, string vacationName, DateTime? startDate, DateTime? endDate, Vacation.VacationType? type, string sortBy = "StartDate", bool orderByParams = false);

        Vacation GetVacationById(int id);

        void CreateVacation (Vacation vacation);

        void UpdateVacation (Vacation vacation);

        void DeleteVacation (int id);
               

        public void UpdateLastMinuteStatus(Vacation vacation);
    }
}
