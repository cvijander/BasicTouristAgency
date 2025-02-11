using BasicTouristAgency.Models;
using BasicTouristAgency.Util.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BasicTouristAgency.Services
{
    public class VacationService : IVacationService
    {
        private readonly BasicTouristAgenctDbContext _dbContext;

        public VacationService (BasicTouristAgenctDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void CreateVacation(Vacation vacation)
        {
            _dbContext.Vacations.Add(vacation);
        }

        public void DeleteVacation(int id)
        {
            Vacation vacation = _dbContext.Vacations.Find(id);
            if(vacation != null)
            {
                _dbContext.Vacations.Remove(vacation);
            }
        }

        public IEnumerable<Vacation> GetAllFilteredVacation(int? minPrice, int? maxPrice, string vacationName, DateTime? startDate, DateTime? endDate, Vacation.VacationType? vacType)

        {
            var vacations = _dbContext.Vacations.AsQueryable();

            if(minPrice.HasValue)
            {
                vacations = vacations.Where(v => v.Price >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                vacations = vacations.Where(v => v.Price <= maxPrice);
            }

            if (!string.IsNullOrEmpty(vacationName))
            {
                vacations = vacations.Where(v => v.VacationName.ToLower().Trim() == vacationName.ToLower().Trim());
            }

            if (startDate.HasValue)
            {
                vacations = vacations.Where(v => v.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                vacations = vacations.Where(v => v.EndDate <= endDate.Value);
            }

            if(vacType.HasValue)
            {
                vacations = vacations.Where(v => v.Type == vacType.Value);
            }

            return vacations.ToList();
        }

      

        public IEnumerable<Vacation> GetAllVacations()
        {
            return _dbContext.Vacations.ToList();
        }

        public Vacation GetVacationById(int id)
        {
            return _dbContext.Vacations.Find(id);
        }

        public List<SelectListItem> GetVacationTypes()
        {
            return VacationUtils.GetVacationTypes();
        }

        public void UpdateVacation(Vacation vacation)
        {
            _dbContext.Vacations.Update(vacation);
        }
    }
}
