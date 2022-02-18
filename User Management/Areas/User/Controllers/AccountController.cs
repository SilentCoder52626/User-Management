using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using User_Management.Areas.User.Models;
using User_Management.Controllers;

namespace User_Management.Areas.User.Controllers
{
    [AllowAnonymous]
    [Area("User")]
    public class AccountController : Controller
    {
        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<AccountController> logger)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            Logger = logger;
        }

        private readonly UserManager<IdentityUser> UserManager;
        private readonly SignInManager<IdentityUser> SignInManager;
        private readonly ILogger<AccountController> Logger;
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = new IdentityUser()
            {
                UserName = vm.Email,
                Email = vm.Email
            };
            var result = await UserManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, false);
                return RedirectToAction(nameof(Index), "Home", new { area = "" });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        public IActionResult Login(string returnUrl = null)
        {
            LoginViewModel model = new()
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var result = await SignInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, false);
            if (result.Succeeded)
            {
                if (!String.IsNullOrEmpty(vm.ReturnUrl))
                {
                    return Redirect(vm.ReturnUrl);
                }
                return RedirectToAction(nameof(Index), "Home", new { area = "" });

            }
            ModelState.AddModelError("", "Email or Password Incorrect.");


            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
        public IActionResult TwitterLogin(string returnUrl)
        {

            var redirectUrl = Url.Action(nameof(ExternalResponse), new { ReturnUrl = returnUrl });
            var properties = SignInManager
                .ConfigureExternalAuthenticationProperties("Twitter", redirectUrl);
            return new ChallengeResult("Twitter", properties);

        }
        public IActionResult GoogleLogin(string returnUrl)
        {

            var redirectUrl = Url.Action(nameof(ExternalResponse), new { ReturnUrl = returnUrl });
            var properties = SignInManager
                .ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);

        }
        public async Task<IActionResult> ExternalResponse(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            LoginViewModel loginViewModel = new()
            {
                ReturnUrl = returnUrl
            };
            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Error From External Provider: {remoteError}");
                return View(nameof(Login), loginViewModel);
            }

            var result = await SignInManager.GetExternalLoginInfoAsync();

            if (result == null)
            {
                ModelState.AddModelError("", $"Error Loading external login information.");
                return View(nameof(Login), loginViewModel);
            }
            var SignInResult = await SignInManager.ExternalLoginSignInAsync(result.LoginProvider, result.ProviderKey, false, true);
            if (SignInResult.Succeeded)
            {
                return Redirect(returnUrl);
            }
            else
            {
                var email = result.Principal.FindFirstValue(ClaimTypes.Email);
                if (email != null)
                {
                    var user = await UserManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new IdentityUser()
                        {
                            UserName = email,
                            Email = email
                        };
                        await UserManager.CreateAsync(user);
                    }
                    await UserManager.AddLoginAsync(user, result);
                    await SignInManager.SignInAsync(user, false);
                    return Redirect(returnUrl);

                }
            }

            return View(nameof(Login), loginViewModel);

        }
    }
}
