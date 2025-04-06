using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebSite.Models;
using System.Threading.Tasks;

namespace MyWebSite.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /UserAdmin
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    RegistrationDate = user.RegistrationDate,
                    Roles = roles.ToList(),
                    OrderCount = user.Orders?.Count ?? 0
                });
            }

            var model = new UserAdminViewModel
            {
                Users = userViewModels,
                TotalUsers = userViewModels.Count,
                NewUsersThisMonth = userViewModels
                    .Count(u => u.RegistrationDate >= DateTime.Now.AddMonths(-1)),
                ActiveUsers = userViewModels.Count(u => u.OrderCount > 0),
                AdminUsers = userViewModels.Count(u => u.Roles.Contains("Admin"))
            };

            return View(model);
        }


        // GET: /UserAdmin/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                Country = user.Country,
                RegistrationDate = user.RegistrationDate,
                Roles = roles.ToList(),
                Orders = user.Orders?.OrderByDescending(o => o.OrderDate).Take(5).ToList(),
                Reviews = user.Reviews?.OrderByDescending(r => r.CreatedAt).Take(5).ToList()
            };

            return View(model);
        }

        // GET: /UserAdmin/Edit/{id}
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                City = user.City,
                State = user.State,
                ZipCode = user.ZipCode,
                Country = user.Country,
                AllRoles = roles.Select(r => r.Name).ToList(),
                UserRoles = userRoles.ToList()
            };

            return View(model);
        }

        // POST: /UserAdmin/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Address = model.Address;
                user.City = model.City;
                user.State = model.State;
                user.ZipCode = model.ZipCode;
                user.Country = model.Country;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update roles if needed
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var rolesToAdd = model.SelectedRoles?.Except(userRoles) ?? new List<string>();
                    var rolesToRemove = userRoles.Except(model.SelectedRoles ?? new List<string>());

                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // GET: /UserAdmin/Delete/{id}
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                RegistrationDate = user.RegistrationDate,
                Roles = roles.ToList()
            };

            return View(model);
        }

        // POST: /UserAdmin/DeleteConfirmed/{id}
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /UserAdmin/ToggleAccountStatus/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAccountStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Toggle lockout status
            if (await _userManager.IsLockedOutAsync(user))
            {
                await _userManager.SetLockoutEndDateAsync(user, null); // Unlock
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)); // Lock
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
