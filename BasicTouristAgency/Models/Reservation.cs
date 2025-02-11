using System.ComponentModel.DataAnnotations;

namespace BasicTouristAgency.Models
{
    public class Reservation :IValidatableObject
    {
        public int ReservationId { get; set; }

        [Required]
        public string UserId { get; set; }

        public User User { get; set; }

        [Required]
        public int VacationId { get; set; } 

        public Vacation Vacation { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateCreatedReservation { get; set; }

        [Required]
        public ReservationStatus Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(DateCreatedReservation > Vacation.StartDate)
            {
                yield return new ValidationResult("Can not make reservation after the startd date has begun ", new[] { nameof(DateCreatedReservation) });
            }


        }

        public enum ReservationStatus
        {
            Created,
            Confirmed, 
            Canceled,
            OnHold,
            InProgress,
            Completed
        }


    }
}
