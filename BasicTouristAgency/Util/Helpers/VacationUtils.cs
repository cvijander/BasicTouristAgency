using BasicTouristAgency.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasicTouristAgency.Util.Helpers
{
    public static class VacationUtils
    {
        public static List <SelectListItem> GetVacationTypes()
        {
            List<SelectListItem> vacationTypes = new List<SelectListItem>();

            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.Summer.ToString(), Text = "Summer Vacation" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.Winter.ToString(), Text = "Winter Vacation" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.EuropeanCities.ToString(), Text = "European Cities" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.NewYear.ToString(), Text = "New Year" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.Cruser.ToString(), Text = "Cruser Trip" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.FarDestinations.ToString(), Text = "Far Destinations" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.LastMinute.ToString(), Text = "Last Minute Deals" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.SpecialOffers.ToString(), Text = "Special offers" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.Wellness.ToString(), Text = "Wellness" });
            vacationTypes.Add(new SelectListItem { Value = Vacation.VacationType.Mountains.ToString(), Text = "Mountains" });

            return vacationTypes;
        }

        public static string GetVacationTypeDisplayName(Vacation.VacationType type)
        {
            return GetVacationTypes().FirstOrDefault(v => v.Value == type.ToString())?.Text ?? type.ToString();
        }

    }
}
