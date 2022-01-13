using ASp.netCore_empty_tutorial.Models;

using ASp.netCore_empty_tutorial.ViewModels;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASp.netCore_empty_tutorial.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this.logger = logger;
        }

        //logout action metod
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }




        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already use ");
            }
        }
        //register action metod
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(AccountRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    City = model.City

                };

                var result = await _userManager.CreateAsync(user, model.Password);


                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmLink = Url.Action("EmailConfirm", "Account", new { userId = user.Id, token = token }, Request.Scheme);
                    var fulPath = $"https://localhost:44389{confirmLink}";
                    //code for sending confirmLink
                    var emailsend = new EmailSend();
                    emailsend.EmailConfirm(fulPath, user.UserName, user.Email);
                    //...

                    if (_signInManager.IsSignedIn(User) && (User.IsInRole("Admin")))
                    {
                        return RedirectToAction("GetUsers", "Administration");
                    }
                    ViewBag.ErrorTitle = "Email not confirmed yest!";
                    ViewBag.ErrorMessage = $"For succesful registrarion you need to confirm email! pleas check your email: {user.Email}";
                    return View("Error");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> EmailConfirm(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User: {user.Email} not found, null references";
                return View("NotFound");
            }

            if (user != null && token != null)
            {
                await _userManager.ConfirmEmailAsync(user, token);
                return View();
            }
            ViewBag.ErrorTitle = "Token null exception!";
            ViewBag.ErrorMessage = $"Invalid Token";
            return View("Error");
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            AccountLoginViewModel model = new AccountLoginViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(AccountLoginViewModel model, string returnUrl)
        {
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();


            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !(await _userManager.IsEmailConfirmedAsync(user)) && (await _userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }

                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View(model);
        }
        //external login providers
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallBack", "Account", new { ReturnUrl = returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallBack(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            AccountLoginViewModel accountmodel = new AccountLoginViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };


            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"We have a error from provider: {remoteError}");
                return View("Login", accountmodel);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "We have a error with info provider");
                return View("Login", accountmodel);
            }

            //check if the user with this provider exists

            var signInresult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            if (signInresult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email not confirmed yet!");
                    return View("Login", accountmodel);
                }
                var model = new ApplicationUser()
                {
                    Email = email,
                    UserName = email
                };

                await _userManager.CreateAsync(model);
                await _userManager.AddLoginAsync(model, info);
                if (!(await _userManager.IsEmailConfirmedAsync(model)))
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(model);
                    var confimLink = Url.Action("EmailConfirm", "Account", new { userId = model.Id, token = token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, confimLink);
                    ViewBag.ErrorTitle = "Registration Succesful";
                    ViewBag.ErrorMessage = $"Before login, eou need to confirm email!";
                    return View("Error");
                }


            }

            ViewBag.ErrorTitle = $"Email claim not received from {info.LoginProvider}";
            ViewBag.ErrorMessage = "pleas contact suport SuportEmail@.com";
            return View("Error");
        }


        //forgot passwprd functionality
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {
                    var genToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetLink = Url.Action("ResetPassword", "Account", new { Email = user.Email, token = genToken }, Request.Scheme);
                    logger.Log(LogLevel.Warning, resetLink);

                    return View("ForgotPasswordConfirmation");
                }
                ModelState.AddModelError(string.Empty, "Invalid Email Adress");
                return View(model);
            }
            return View(model);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string Email, string token)
        {
            if (token == null || Email == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid reset token or email adress");

            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.token, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return View("ResetPasswordSucces");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }
                ModelState.AddModelError(string.Empty, "Invalid user!");
                return View(model);
            }
            return View(model);
        }

        //Change User Password Action Method
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            var userHasPassword = await _userManager.HasPasswordAsync(user);
            if (!userHasPassword)
            {
                return RedirectToAction("AddPassword");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);

                    ViewBag.Message = User.Identity.Name;
                    return View("SuccesChangePassword");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);

                }
                return View();
            }
            return View(model);

        }

        //add local password for external user authentication, Google or Facebook

        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            var userHasPassword = await _userManager.HasPasswordAsync(user);
            if (userHasPassword)
            {
                return RedirectToAction("ChangePassword");
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var result = await _userManager.AddPasswordAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    return View("AddPasswordConfirmation");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
            return View(model);

        }
    }
}
