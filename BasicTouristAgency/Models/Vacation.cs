using System.ComponentModel.DataAnnotations;

namespace BasicTouristAgency.Models
{
    public class Vacation :IValidatableObject
    {
        public int VacationId { get; set; }

        [Required]
        [StringLength(50)]
        public string VacationName { get; set; }

        [Required]
        [StringLength(450)]
        public string VacationDescription { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please select a vacation type.")]
        public VacationType Type { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(EndDate < StartDate)
            {
                yield return new ValidationResult("Can not have a end date before start date", new[] { nameof(EndDate) });
            }

            if (Price <= 0)
            {
                yield return new ValidationResult("can not have a price 0 or below", new[] { nameof(Price) });
            }
        }

        public enum VacationType
        {
            Summer,
            Winter,
            EuropeanCities,
            NewYear,
            Cruser,
            FarDestinations,
            LastMinute,
            SpecialOffers,
            Wellness
        }
    }
}
