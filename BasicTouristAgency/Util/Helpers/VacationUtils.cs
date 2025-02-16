using BasicTouristAgency.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasicTouristAgency.Util.Helpers
{
    public static class VacationUtils
    {
        public static List <SelectListItem> GetVacationTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = Vacation.VacationType.Summer.ToString(), Text = "Summer Vacation" },
                new SelectListItem { Value = Vacation.VacationType.Winter.ToString() , Text = "Winter Vacation" },
                new SelectListItem { Value = Vacation.VacationType.EuropeanCities.ToString(), Text = "European Cities" },
                new SelectListItem { Value = Vacation.VacationType.NewYear.ToString(), Text = "New Year" },
                new SelectListItem { Value = Vacation.VacationType.Cruser.ToString(), Text = "Cruser Trip" },
                new SelectListItem { Value = Vacation.VacationType.FarDestinations.ToString(), Text = "Far Destinations" },
                new SelectListItem { Value = Vacation.VacationType.LastMinute.ToString(), Text = "Last Minute Deals" },
                new SelectListItem { Value = Vacation.VacationType.SpecialOffers.ToString(), Text = "Special offers" },
                new SelectListItem { Value = Vacation.VacationType.Wellness.ToString(), Text = "Wellness" },
                new SelectListItem { Value = Vacation.VacationType.Mountains.ToString(), Text = "Mountains"}
            };
        }
       
    }
}
