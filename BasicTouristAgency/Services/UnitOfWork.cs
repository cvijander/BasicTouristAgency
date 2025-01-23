namespace BasicTouristAgency.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BasicTouristAgenctDbContext _dbContext;

        public UnitOfWork(BasicTouristAgenctDbContext dbContext)
        {
            _dbContext = dbContext;

            
            UserService = new UserService(dbContext);

            VacationService = new VacationService(dbContext);

            ReservationService = new ReservationService(dbContext);
        }

        public IUserService UserService { get; private set; }

        public IVacationService VacationService { get; private set; }

        public IReservationService ReservationService { get; private set; }

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }
    }
}
