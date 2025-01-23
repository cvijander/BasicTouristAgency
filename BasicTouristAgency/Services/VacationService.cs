using BasicTouristAgency.Models;

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
