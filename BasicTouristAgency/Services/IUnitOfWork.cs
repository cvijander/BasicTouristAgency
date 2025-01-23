namespace BasicTouristAgency.Services
{
    public interface IUnitOfWork
    {
        void SaveChanges();

        IUserService UserService { get; }

        IVacationService VacationService { get; }

        IReservationService ReservationService { get; }
    }
}
