using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BasicTouristAgency.Models
{
   

    public class User :IdentityUser
    {

        [StringLength(50,MinimumLength = 2,ErrorMessage ="First name must be between 2 and 50 characters" )]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string? LastName { get; set; }

       
       
    }
}
