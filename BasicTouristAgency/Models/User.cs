using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BasicTouristAgency.Models
{
   

    public class User :IdentityUser
    {

       
        [StringLength(50, ErrorMessage = "First name must be at most 50 characters.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters.")]
        public string? FirstName { get; set; }

        
        [StringLength(50, ErrorMessage = "Last name must be at most 50 characters.")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters.")]
        public string? LastName { get; set; }

       
       
    }
}
