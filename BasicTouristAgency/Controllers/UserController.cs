using BasicTouristAgency.Models;
using BasicTouristAgency.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BasicTouristAgency.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Profile()
        {
            IEnumerable<User> users = _userManager.Users.ToList();

            IEnumerable<IdentityRole> roles = _roleManager.Roles.ToList();

            var user = _userManager.GetUserAsync(User).Result;

            UsersRoles ur = new UsersRoles();
            ur.Roles = roles;
            ur.Users = users;
            ur.UserName = user.UserName;
            ur.Email = user.Email;

            return View(ur);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UsersRoles usersRoles)
        {
            Console.WriteLine("Profie post method called");

            if (string.IsNullOrEmpty(usersRoles.SelectedUser) || string.IsNullOrEmpty(usersRoles.SelectedRole))
            {
                ModelState.AddModelError("", "You must select both a user and a rode");
                return RedirectToAction("Profile");
            }

            var user = await _userManager.FindByIdAsync(usersRoles.SelectedUser);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return RedirectToAction("Profile");
            }

            var roleExists = await _roleManager.RoleExistsAsync(usersRoles.SelectedRole);
            if (!roleExists)
            {
                ModelState.AddModelError("", "Role does not exist");
                return RedirectToAction("Profile");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in currentRoles)
            {
                await _userManager.RemoveFromRoleAsync(user, role);

            }


            var result = await _userManager.AddToRoleAsync(user, usersRoles.SelectedRole);

            if (result.Succeeded)
            {
                TempData["Success"] = "Role updated succesfully";
            }
            else
            {
                TempData["Error"] = "Failed to update role";
            }
            return RedirectToAction("Profile");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRoles()
        {
            var users = _userManager.Users.ToList();
            var roles = _roleManager.Roles.ToList();

            var userRoles = new List<UsersRoles>();

            foreach (User user in users)
            {
                var userRoleList = await _userManager.GetRolesAsync(user);
                var userRole = userRoleList.FirstOrDefault();

                userRoles.Add(new UsersRoles
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    SelectedRole = userRole,
                    Users = users,
                    Roles = roles,
                    SelectedUser = user.Id
                });
            }

            return View(userRoles);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(UsersRoles model)
        {
            Console.WriteLine("UpdateUserRole called!");

            if (string.IsNullOrEmpty(model.SelectedUser) || string.IsNullOrEmpty(model.SelectedRole))
            {
                TempData["Error"] = "Invalid user or role selected!";
                return RedirectToAction("ManageRoles");
            }

            var user = await _userManager.FindByIdAsync(model.SelectedUser);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("ManageRoles");
            }

            var roleExists = await _roleManager.RoleExistsAsync(model.SelectedRole);
            if (!roleExists)
            {
                TempData["Error"] = "Role does not exist!";
                return RedirectToAction("ManageRoles");
            }

           
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            
            var result = await _userManager.AddToRoleAsync(user, model.SelectedRole);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Role for {user.UserName} updated to {model.SelectedRole}!";
            }
            else
            {
                TempData["Error"] = "Failed to update role!";
            }

            return RedirectToAction("ManageRoles");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
