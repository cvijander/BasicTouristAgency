using BasicTouristAgency.Models;
using Microsoft.AspNetCore.Identity;

namespace BasicTouristAgency.ViewModel
{
    public class ManageRoleViewModel
    {

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string SelectedRole { get; set; }

        public List<string> AvailableRoles { get; set; }           
      

        
    }
}
