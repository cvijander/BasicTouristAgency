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

        public IEnumerable<Vacation> GetAllFilteredVacation(int? minPrice, int? maxPrice, string vacationName, DateTime? startDate, DateTime? endDate, Vacation.VacationType? vacType, string sortBy = "StartDate", bool descending = false)

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
                vacations = vacations.Where(v => v.VacationName.ToLower().Trim().Contains(vacationName.ToLower().Trim()));
            }

            if (startDate.HasValue)
            {
                vacations = vacations.Where(v => v.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                vacations = vacations.Where(v => v.EndDate <= endDate.Value);
            }
            if (endDate == null) 
            {
                vacations = vacations.Where(v => v.EndDate >= DateTime.Now);
            }

            if (vacType.HasValue)
            {
                vacations = vacations.Where(v => v.Type == vacType.Value);
            }
            // sortiranje 

            switch(sortBy)
            {
                case "Price":
                    {
                        vacations = descending ? vacations.OrderByDescending(v => v.Price) : vacations.OrderBy(v => v.Price);
                    }
                    break;

                case "EndDate":
                    {
                        vacations = descending ? vacations.OrderByDescending(v => v.EndDate) : vacations.OrderBy(v => v.EndDate);
                    }
                    break;

                default:
                    {
                        vacations = descending ? vacations.OrderByDescending(v => v.StartDate) : vacations.OrderBy(v => v.StartDate);
                    }
                    break;

            }

            //if(minPrice.HasValue && !maxPrice.HasValue)
            //{
            //    vacations = vacations.OrderBy(v => v.Price);
            //}

            //if (!minPrice.HasValue && maxPrice.HasValue)
            //{
            //    vacations = vacations.OrderByDescending(v => v.Price);
            //}

            //if (minPrice.HasValue && maxPrice.HasValue)
            //{
            //    vacations = vacations.OrderBy(v => v.Price);
            //}

            //if (startDate.HasValue && !endDate.HasValue)
            //{
            //    vacations = vacations.OrderBy(v => v.StartDate);
            //}

            //if (!startDate.HasValue && endDate.HasValue)
            //{
            //    vacations = vacations.OrderByDescending(v => v.EndDate);
            //}

            //if (startDate.HasValue && endDate.HasValue)
            //{
            //    vacations = vacations.OrderBy(v => v.StartDate);
            //}
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
