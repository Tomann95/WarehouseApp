using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Models;
using WarehouseApp.Models.ViewModels;

namespace WarehouseApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserWithRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Roles = roles.Any() ? string.Join(", ", roles) : "(brak ról)"
                });
            }

            return View(model);
        }

        // GET: Users/EditRoles/5
        public async Task<IActionResult> EditRoles(string id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                IsAdministrator = userRoles.Contains("Administrator"),
                IsWarehouseWorker = userRoles.Contains("Pracownik magazynu"),
                IsWarehouseManager = userRoles.Contains("Kierownik magazynu"),
                IsAuditor = userRoles.Contains("Audytor")
            };

            return View(model);
        }

        // POST: Users/EditRoles/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var allRoles = new[] { "Administrator", "Pracownik magazynu", "Kierownik magazynu", "Audytor" };

            // zdejmujemy wszystkie role z tej czwórki
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Where(r => allRoles.Contains(r));
            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            // zakładamy nowe role wg checkboxów
            var newRoles = new List<string>();
            if (model.IsAdministrator) newRoles.Add("Administrator");
            if (model.IsWarehouseWorker) newRoles.Add("Pracownik magazynu");
            if (model.IsWarehouseManager) newRoles.Add("Kierownik magazynu");
            if (model.IsAuditor) newRoles.Add("Audytor");

            if (newRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, newRoles);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

