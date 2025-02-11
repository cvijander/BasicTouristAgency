using BasicTouristAgency.Models;
using Microsoft.AspNetCore.Identity;

namespace BasicTouristAgency.ViewModel
{
    public class UsersRoles
    {
        public IEnumerable<User> Users { get; set; }

        public IEnumerable<IdentityRole> Roles { get; set; }

        public string SelectedUser { get; set; }    

        public string SelectedRole { get; set; }

        public string UserName { get; set;  }

        public string Email { get; set; }
    }
}
