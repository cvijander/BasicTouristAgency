using BasicTouristAgency.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BasicTouristAgency
{

    public class BasicTouristAgenctDbContext :IdentityDbContext<User>
    {
        public static readonly ILoggerFactory loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        public BasicTouristAgenctDbContext(DbContextOptions<BasicTouristAgenctDbContext> options):base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Vacation> Vacations { get; set; }

       
    }
}
