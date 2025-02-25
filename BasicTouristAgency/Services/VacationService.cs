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

        public void UpdateLastMinuteStatus(Vacation vacation)
        {
            DateTime maxLastMinuteStart = DateTime.Now.AddDays(10).Date;

            if(vacation.Type != Vacation.VacationType.LastMinute && vacation.StartDate < maxLastMinuteStart)
            {
                vacation.Type = Vacation.VacationType.LastMinute;
                _dbContext.Vacations.Update(vacation);
            }
        }

        public void DeleteVacation(int id)
        {
            Vacation vacation = _dbContext.Vacations.Find(id);
            if(vacation != null)
            {
                _dbContext.Vacations.Remove(vacation);
            }
        }

        public IEnumerable<Vacation> GetAllFilteredVacation(int? minPrice, int? maxPrice, string vacationName, DateTime? startDate, DateTime? endDate, Vacation.VacationType? vacType, string sortBy = "StartDate", bool orderByParams = false)

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
                vacations = vacations.Where(v => v.VacationName.Contains(vacationName.Trim()));
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
                        if(orderByParams)
                        {
                            vacations = vacations.OrderByDescending(v => v.Price);
                        }
                        else
                        {
                            vacations = vacations.OrderBy(v => v.Price);
                        }
                        
                    }
                    break;

                case "EndDate":
                    {
                        if(orderByParams)
                        {
                            vacations = vacations.OrderByDescending(v => v.EndDate);
                        }
                        else
                        {
                            vacations = vacations.OrderBy(v => v.EndDate);
                        }
                        
                    }
                    break;

                default:
                    {
                        if(orderByParams)
                        {
                            vacations = vacations.OrderByDescending(v => v.StartDate);
                        }
                        else
                        {
                            vacations = vacations.OrderBy(v => v.StartDate);
                        }
                        
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

            foreach (var vacation in vacations)
            {
                if (vacation.Type != Vacation.VacationType.LastMinute && vacation.StartDate < DateTime.Now.AddDays(10))
                {
                    vacation.Type = Vacation.VacationType.LastMinute;
                    _dbContext.Vacations.Update(vacation);
                }
            }
            _dbContext.SaveChanges();


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

        

        public void UpdateVacation(Vacation vacation)
        {
            _dbContext.Vacations.Update(vacation);
        }
    }
}
