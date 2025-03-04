﻿using System.ComponentModel.DataAnnotations;

namespace BasicTouristAgency.Models
{
    public class Vacation :IValidatableObject
    {
        public int VacationId { get; set; }

        [Required(ErrorMessage = "Vacation name is required.")]
        [StringLength(50, ErrorMessage = "Vacation name must be at most 50 characters.")]
        [MinLength(2,ErrorMessage = "Vacation name must be at least 2 characters.")]
        public string VacationName { get; set; }

        [Required(ErrorMessage = "Vacation description is required.")]
        [StringLength(450, ErrorMessage = "Vacation description must be at most 450 characters.")]
        [MinLength(2,ErrorMessage = "Vacation description must be at least 2 characters.")]
        public string VacationDescription { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 20000, ErrorMessage = "Price must be between 0.01 and 20000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please select a vacation type.")]
        public VacationType Type 
        {
            get {
                DateTime maxLastMinuteStart = DateTime.Now.AddDays(10).Date;
                if (_type !=Vacation.VacationType.LastMinute && StartDate < maxLastMinuteStart)
                {
                    return VacationType.LastMinute;
                }
                return _type;
            }
            set 
            {
                _type = value;
            }
        }
        private VacationType _type;



        // aditional validation with time 
        // vacation expired 
        public bool IsExpired()
        {
            return EndDate < DateTime.Today;
        }

        // vacation has starded 
        public bool HasStarted()
        {
            return StartDate <= DateTime.Today;
        }


        // available for reservation if has not expired and time od start is greater that tis thime right now 
        public bool IsAvailableForReservation()
        {
            return !IsExpired() && !HasStarted();
        }


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

            //variables 

            int startMonth = StartDate.Month;
            int endMonth = EndDate.Month;
            int maxDuration = 60;

            DateTime maxLastMinuteSrart = DateTime.Now.AddDays(10).Date;
            DateTime minLastMinuteStart = DateTime.Now.Date;
            


            // for changing every vacation to last minute offer 

             if (Type !=Vacation.VacationType.LastMinute &&  StartDate < maxLastMinuteSrart)
             {
                //yield return new ValidationResult("This vacation should be marked as 'Last minute'.", new[] { nameof(VacationType) });
                Type =Vacation.VacationType.LastMinute;
             }

            switch (Type)
            {
                case Vacation.VacationType.NewYear: 
                    if(startMonth !=12)
                    {
                        yield return new ValidationResult("New year vacation can only start in december" , new[] { nameof(StartDate)});
                    }
                    if(endMonth > 1 || (endMonth == 1 && EndDate.Day > 10))
                    {
                        yield return new ValidationResult("New year vacation can only end in January (max 10th)", new[] { nameof(EndDate) });
                    }
                    break;

                case Vacation.VacationType.Summer:
                    if(startMonth < 5 || startMonth > 9)
                    {
                        yield return new ValidationResult("Summer vacation can only be betwnen may and september", new[] { nameof(StartDate) });
                    }
                    if (endMonth < 5 || endMonth > 10)
                    {
                        yield return new ValidationResult("Summer vacation can only end betwen may and oktober", new[] { nameof(EndDate) });
                    }
                    break;

                case Vacation.VacationType.Winter:
                    if(startMonth > 3 && startMonth <= 11)
                    {
                        yield return new ValidationResult("Winter vacation can only start only in novbmer or december, and end in mart or april", new[] { nameof(StartDate) });

                    }

                    if(endMonth < 11 && endMonth > 4 )
                    {
                        yield return new ValidationResult("Winter vacation can only end between Nobemmber and April", new[] { nameof(EndDate) });
                    }
                    break;

                case Vacation.VacationType.LastMinute:
                    {
                        if(StartDate  < minLastMinuteStart || StartDate > maxLastMinuteSrart )
                        {
                            yield return new ValidationResult("Last minute offer can not start in more than 10 days ", new[] { nameof(StartDate)});
                        }
                        
                    }
                    break;

                case Vacation.VacationType.Cruser:
                    {
                        if((EndDate - StartDate).TotalDays > 180)
                        {
                            yield return new ValidationResult("Cruiser trip ca not last longer than 180 days", new[] { nameof(EndDate) });
                        }
                    }
                    break;


                case Vacation.VacationType.FarDestinations:
                    {
                        if((EndDate -StartDate).TotalDays < 10)
                        {
                            yield return new ValidationResult("Far destinations vacation must be at least 10 days", new[] { nameof(EndDate) });
                        }
                    }
                    break;

                case Vacation.VacationType.SpecialOffers:
                    {
                        if(Price > 1000 )
                        {
                            yield return new ValidationResult("Specila price offers can not exceed 1000 ", new[] { nameof(Price) });
                        }
                    }
                    break;


                case Vacation.VacationType.Wellness:
                    {
                        if (Price < 100)
                        {
                            yield return new ValidationResult("Wellnes vacation must be at leat more from 100", new[] { nameof(Price) });
                        }
                        if((EndDate - StartDate ).TotalDays < 25)
                        {
                            yield return new ValidationResult("Wellnes vacation can not last longer than 25 days", new[] { nameof(Price) });
                        }
                    }
                    break;

                case Vacation.VacationType.Mountains:
                    {
                        if (startMonth < 3 || startMonth > 11) 
                        {
                            yield return new ValidationResult("Mountains can start from March until novmber", new[] { nameof(StartDate) }); 
                        }
                        if ((EndDate -StartDate ).TotalDays > 30)
                        {
                            yield return new ValidationResult("Mountaiun vacation can onl be 30 days long", new[] { nameof(EndDate) });
                        }

                    }
                    break;

            }

            if((EndDate - StartDate).TotalDays > maxDuration)
            {
                yield return new ValidationResult($"Vacation duration cannot exceed {maxDuration} days - you have a very good holiday",new[] { nameof(EndDate) });
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
            Wellness,
            Mountains
        }
    }
}
