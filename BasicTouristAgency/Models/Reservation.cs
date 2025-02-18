using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BasicTouristAgency.Models
{
    public class Reservation :IValidatableObject
    {
        public int ReservationId { get; set; }

        [Required]
        public string UserId { get; set; }

        [BindNever]
        [ValidateNever]
        public User? User { get; set; }

        [Required]
        public int VacationId { get; set; }

        [BindNever]
        [ValidateNever]
        public Vacation? Vacation { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateCreatedReservation { get; set; }

        [Required]
        public ReservationStatus Status { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Vacation == null)
            {
                //yield return new ValidationResult("Vacation is missing", new[] { nameof(Vacation) });
                yield break;
            }

            if (DateCreatedReservation.Date >= Vacation.StartDate.Date)
            {
                yield return new ValidationResult("Can not make reservation after the startd date has begun ", new[] { nameof(DateCreatedReservation) });
            }
            if (DateCreatedReservation.Date < DateTime.Today.Date)
            {
                yield return new ValidationResult("The reservation can not be in the past", new[] { nameof(DateCreatedReservation) });
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
