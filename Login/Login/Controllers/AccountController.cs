using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Login.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Login Page
        public IActionResult Login()
        {
            return View();
        }

        // Handle Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Email and Password are required.";
                return View();
            }
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !(await _userManager.CheckPasswordAsync(user, password)))
            {
                ViewBag.ErrorMessage = "Invalid email or password!";
                return View();
            }


            // Ensure the account is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ViewBag.ErrorMessage = "Please confirm your email before logging in.";
                return View();
            }
      
            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Dashboard", "Admin");

                if (roles.Contains("Employee"))
                    return RedirectToAction("Dashboard", "Employee");

                return RedirectToAction("Login");
            }

            ViewBag.ErrorMessage = "Login failed!";
            return View();
        }

        // Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Change Password (Authenticated Users Only)
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldpassword, string newpassword ,string confrompassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found!";
                return View();
            }

            var result = await _userManager.ChangePasswordAsync(user, oldpassword, newpassword);

           

            if (newpassword == confrompassword)
            {
                if (result.Succeeded)
                {
                    ViewBag.Message = "Password changed successfully!";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "Error changing password!";
                }
            }
            else 
            {
                ViewBag.ErrorMessage = "Newpassword and Confrompassword should be same!";
            }
                return View();
        }

       
    }
}
