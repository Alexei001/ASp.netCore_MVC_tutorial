using ASp.netCore_empty_tutorial.Models;
using ASp.netCore_empty_tutorial.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdministrationController : Controller
    {
        protected readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
           ILogger<AdministrationController> logger)
        {
            _roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleAdministrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole newRole = new IdentityRole() { Name = model.RoleName };

                IdentityResult result = await _roleManager.CreateAsync(newRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("GetRoles", "Administration");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    };
                }
            }
            return View(model);
        }
        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"This role id: {id} can not be found";
                return View("NotFound");
            }
            var model = new EditRoleAdministrationViewModel()
            {
                Id = role.Id,
                Name = role.Name
            };

            foreach (var user in userManager.Users.ToList())
            {
                var result = await userManager.IsInRoleAsync(user, role.Name);
                if (result)
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);

        }
        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRole(EditRoleAdministrationViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"This role id: {model.Id} can not be Edit";
                return View("NotFound");
            }
            else
            {
                role.Name = model.Name;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("GetRoles", "Administration");
                }
            }
            return View(model);

        }



        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"This role id: {Id} can not be found";
                return View("NotFound");
            }
            try
            {

                IdentityResult result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("GetRoles", "Administration");
                }
                else
                {
                    ViewBag.ErrorMessage = $"delete error: {result.Errors.FirstOrDefault()}";
                    return View("NotFound");
                }
            }
            catch (DbUpdateException ex)
            {
                logger.LogError($"Trying to delete {role.Name} role with multiple users", ex);
                ViewBag.ErrorTitle = $"{role.Name} role ins't accept to delete!";
                ViewBag.ErrorMessage = $"You are trying to delete Role: {role.Name} that has multiple users, delete all users from this role and try again!";
                return View("Error");
            }

        }
        [HttpGet]
        public IActionResult GetRoles()
        {
            IEnumerable<IdentityRole> roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

        [HttpPost]

        public async Task<IActionResult> DeleteUser(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {

                ViewBag.ErrorMessage = $"This user id: {Id} can not be found";
                return View("NotFound");
            }
            else
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("GetUsers");
                }


                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);

                }
                return View("GetUsers");
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {

                ViewBag.ErrorMessage = $"This user id: {Id} can not be found";
                return View("NotFound");
            }
            else
            {

                var model = new EditUserAdministrationViewModel()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    City = user.City
                };
                var claims = await userManager.GetClaimsAsync(user);
                var roles = await userManager.GetRolesAsync(user);
                model.Claims = claims.Select(c => c.Value).ToList();
                foreach (var role in roles)
                {
                    model.Roles.Add(role);
                }
                return View(model);
            }

        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserAdministrationViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            if (user == null)
            {

                ViewBag.ErrorMessage = $"This user id: {model.Id} can not be found";
                return View("NotFound");
            }
            else
            {
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.City = model.City;

                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("GetUsers");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View(model);
                }

            }

        }

        //add user in Roles

        [HttpGet]
        public async Task<IActionResult> AddInRoleUser(string roleId)
        {
            ViewBag.RoleId = roleId;

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"This role id: {roleId} can not be found";
                return View("NotFound");
            }
            ViewBag.RoleName = role.Name;
            var model = new List<UserRoleAdministrationViewModel>();

            foreach (var user in userManager.Users.ToList())
            {
                var userRoleViewModel = new UserRoleAdministrationViewModel()
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsEnrole = true;
                }
                else
                {
                    userRoleViewModel.IsEnrole = false;
                }
                model.Add(userRoleViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddInRoleUser(List<UserRoleAdministrationViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"This role id: {roleId} can not be found";
                return View("NotFound");
            }


            foreach (var user in model)
            {
                var applicationUser = await userManager.FindByIdAsync(user.UserId);
                var result = await userManager.IsInRoleAsync(applicationUser, role.Name);

                if ((user.IsEnrole) && !(result))
                {
                    await userManager.AddToRoleAsync(applicationUser, role.Name);
                }
                else if (!(user.IsEnrole) && (result))
                {
                    await userManager.RemoveFromRoleAsync(applicationUser, role.Name);
                }
                else
                {
                    continue;
                }
            }

            return RedirectToAction("EditRole", new { id = role.Id });
        }

        //add role in Users


        [HttpGet]
        public async Task<IActionResult> AddInUserRoles(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id:{userId} not ofund!";
                return View("NotFound");
            }
            var model = new List<RoleUserAdministrationViewModel>();

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.UserId = user.Id;
            ViewBag.UserName = user.UserName;
            foreach (var role in roles)
            {
                var userRole = new RoleUserAdministrationViewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRole.IsSelected = true;
                }
                else
                {
                    userRole.IsSelected = false;
                }
                model.Add(userRole);
            }
            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> AddInUserRoles(List<RoleUserAdministrationViewModel> models, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"This role id: {userId} can not be found";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can't remove user from roles");
                return View(models);
            }
            result = await userManager.AddToRolesAsync(user, models.Where(x => x.IsSelected).Select(rn => rn.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can't add user to roles");
                return View(models);
            }

            return RedirectToAction("EditUser", new { Id = user.Id });
        }

        //add in User Claims

        [HttpGet]
        public async Task<IActionResult> AddInUserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id:{userId} not ofund!";
                return View("NotFound");
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user);
            var model = new UserClaimsViewModel()
            {
                UserId = user.Id
            };
            ViewBag.UserName = user.UserName;
            foreach (var claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim()
                {
                    ClaimType = claim.Type
                };
                if (existingUserClaims.Any(ext => ext.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }
                model.Claims.Add(userClaim);
            }
            return View(model);

        }



        [HttpPost]
        public async Task<IActionResult> AddInUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id:{model.UserId} not ofund!";
                return View("NotFound");
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, existingUserClaims);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"Can't remove claims from user {user.UserName}");
                return View(model);
            }
            var filterclaims = model.Claims.Where(x => x.IsSelected);
            var claims = filterclaims.Select(cl => new Claim(cl.ClaimType, cl.ClaimType));
            result = await userManager.AddClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"Can't remove claims from user {user.UserName}");
                return View(model);
            }
            return RedirectToAction("EditUser", new { Id = model.UserId });
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
