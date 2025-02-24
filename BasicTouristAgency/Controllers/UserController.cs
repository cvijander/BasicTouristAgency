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



        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRoles(string firstName,string lastName, string email, int page = 1)
        {
            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(firstName))
            {
                users = users.Where(u => u.FirstName.Contains(firstName.Trim()));
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                users = users.Where(u => u.FirstName.Contains(lastName.Trim()));
            }

            if (!string.IsNullOrEmpty(email))
            {

                var user = await _userManager.Users.SingleOrDefaultAsync(u => u.Email == email.Trim());

                if (user == null)
                {
                    return View("NotFound");
                }

                PaginationViewModel<User> pvmsSingleUser = new PaginationViewModel<User>();
                pvmsSingleUser.TotalCount = 1;
                pvmsSingleUser.CurrentPage = 1;
                pvmsSingleUser.PageSize = 1;
                pvmsSingleUser.Collection = new List<User> { user };
                
                return View(pvmsSingleUser);
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

        public async Task<IActionResult> ManageRole(string id )
        {
            if (string.IsNullOrEmpty(id))
            {

                TempData["Error"] = "Invalid user id";
                return RedirectToAction("ManageRoles");
            }

            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("ManageRoles");
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);


            ManageRoleViewModel mrvm = new ManageRoleViewModel();

            mrvm.UserId = user.Id;
            mrvm.UserName = user.UserName;
            mrvm.Email = user.Email;
            mrvm.AvailableRoles = roles.Select(r => r.Name).ToList();
            mrvm.SelectedRole = userRoles.FirstOrDefault();
            
            return View(mrvm);

        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> UpdateManageRole(string userId, string SelectedRole)
        {
            if(string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Invalid user id";
                return RedirectToAction("ManageRoles");
            }

            if (string.IsNullOrEmpty(SelectedRole))
            {
                TempData["Error"] = "Invalid  role selected!";
                return RedirectToAction("ManageRoles");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found!";
                return RedirectToAction("ManageRoles");
            }

            var roleExists = await _roleManager.RoleExistsAsync(SelectedRole);
            if (!roleExists)
            {
                TempData["Error"] = "Role does not exist!";
                return RedirectToAction("ManageRoles");
            }

           
            var currentRoles = await _userManager.GetRolesAsync(user);

            if(currentRoles.Contains(SelectedRole))
            {
                TempData["Message"] = "User is already in the slecetred role";
                return RedirectToAction("ManageRoles");
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            
            var result = await _userManager.AddToRoleAsync(user, SelectedRole);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Role for {user.UserName} updated to {SelectedRole}!";
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
            return View("~/Views/Shared/NotFound.cshtml"); 
        }
    }
}
