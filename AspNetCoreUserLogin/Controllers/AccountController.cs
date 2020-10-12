using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspNetCoreUserLogin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace AspNetCoreUserLogin.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UserLoginModel userLoginModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.AppUsers
                    .AsNoTracking()
                    .Where(a => a.UserName.ToUpper() == userLoginModel.UserName.ToUpper())
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid User.");
                    return View(userLoginModel);
                }

                CustomPasswordHasher customPasswordHasher = new CustomPasswordHasher();

                if (!customPasswordHasher.VerifyPassword(user.Password, userLoginModel.Password))
                {
                    ModelState.AddModelError(string.Empty, "Invalid Password.");
                    return View(userLoginModel);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("FullName", user.FullName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties { IsPersistent = false }
                    );

                return RedirectToAction("Index", "Home");
            }
            return View(userLoginModel);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }
    }
}
