using System.ComponentModel.DataAnnotations;

namespace BasicTouristAgency.Util.Helpers
{
    public static class EnumHelper
    {
        // metoda za dobijanje  Dispol name vrenodsti enuma
        public static string GetDisplayName(Enum enumValue)
        {
            // pristup polju za svaku datu vrednosti enuma

            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            // dohvati display atribute ako postoji
            var displayAttribute = fieldInfo
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;
            
            // vrati displayy name ili ime enumna ako atribut ne postoji
            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
