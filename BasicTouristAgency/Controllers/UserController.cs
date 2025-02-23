using AspNetCoreGeneratedDocument;
using BasicTouristAgency.Models;
using BasicTouristAgency.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            // trnutno prijavljen korisnik 
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }           
                       

            return View(user);
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
            if(currentRoles.Any())
            {
                foreach (var role in currentRoles)
                {
                    await _userManager.RemoveFromRoleAsync(user, role);

                }
            }
           
            if(!await _userManager.IsInRoleAsync(user,usersRoles.SelectedRole))
            {
                var result = await _userManager.AddToRoleAsync(user, usersRoles.SelectedRole);

                if (result.Succeeded)
                {
                    TempData["Success"] = "Role updated succesfully";
                }
                else
                {
                    TempData["Error"] = "Failed to update role";
                }
            }
           

            else
            {
                TempData["Error"] = "User already has this role";
                return RedirectToAction("Profile");
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

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Index(string firstName, string lastName, string email, int page = 1)
        {
            var users = _userManager.Users.AsQueryable();

            if(!string.IsNullOrEmpty(firstName))
            {
                users = users.Where(u => u.FirstName.Contains(firstName.Trim()));
            }

            if(!string.IsNullOrEmpty(lastName))
            {
                users = users.Where(u => u.LastName.Contains(lastName.Trim()));
            }

            if(!string.IsNullOrEmpty(email))
            {
               
                var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Email == email.Trim());

                if (user == null)
                {
                    return View("NotFound");
                }

                PaginationViewModel<User> singleUserPagination = new PaginationViewModel<User>
                {
                    TotalCount = 1,
                    CurrentPage = 1,
                    PageSize = 1,
                    Collection =  new List<User> { user }
                };
                return View(singleUserPagination);
            }

            PaginationViewModel<User> pgvmUser = new ViewModel.PaginationViewModel<User>();

            pgvmUser.TotalCount = users.Count();
            pgvmUser.CurrentPage = page;
            pgvmUser.PageSize = 5;

            users = users.Skip(pgvmUser.PageSize * (pgvmUser.CurrentPage - 1)).Take(pgvmUser.PageSize);
            pgvmUser.Collection = users;


            Dictionary<string, string> userRoles = new Dictionary<string, string>();
            foreach (var user in pgvmUser.Collection)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.Any() ? roles.First() : "Tourist";
            }
            ViewBag.UserRoles = userRoles;

            return View(pgvmUser);
        
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if(id == null)
            {
                return View("NotFound");
            }

            var user = await _userManager.FindByIdAsync(id);

            if(user == null)
            {
                return View("NotFound");
            }
            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> ConfirmDelete(string id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var user = await _userManager.FindByIdAsync(id);

            if(user == null)
            {
                return View("NotFound");
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return View("NotFound");
            }

            if (user.Id == currentUser.Id)
            {
                TempData["Error"] = "You can not delte yourself";
                return RedirectToAction("Index");
            }

            var result = await _userManager.DeleteAsync(user);
            if(result.Succeeded)
            {
                TempData["Success"] = "User deleted succeesfully";
            }
            else
            {
                TempData["Error"] = "Error deleting user";
            }

            return RedirectToAction("Index");     

            
        }

               

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return View("NotFound");
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return View("NotFound");
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // uzimanje korinika iz baze posto model nema sve podatke 
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


        public IActionResult TestNotFound()
        {
            return View("~/Views/Shared/NotFound.cshtml"); // Direktno navodi putanju
        }
    }
}
