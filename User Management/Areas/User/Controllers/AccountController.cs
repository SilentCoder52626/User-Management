using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using User_Management.Areas.User.Models;
using User_Management.Controllers;

namespace User_Management.Areas.User.Controllers
{
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

        public IActionResult Login(string? returnUrl = null)
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
    }
}
