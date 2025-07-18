using Login.Data;
using Login.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Login.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context; 

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

       
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] string fullName, [FromForm] string email, [FromForm] string password, [FromForm] string role,string confrompassword, string employeeId)

        {
            
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ViewBag.ErrorMessage = "All fields are required!";
                return View();
            }
            if (password != confrompassword)
            {
                ViewBag.ErrorMessage="Password and Confrompassword should be same";
            }
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "User already exists!";
                return View();
            }

            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                string EmployeeId;
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);
                

                if (role == "Employee")

                 {
                
                     employeeId = GenerateEmployeeId(fullName);

                    var employee = new Employee
                    {
                        Name = fullName,
                        Email = email,
                        Role = role,
                        EmployeeId =employeeId,
                        UniqueIdentifier = Guid.Parse(user.Id) 
                    };

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();
                }

                TempData["success"] = $"{employeeId}  Created Successfully!";

                //ViewBag.Message = $"{employeeId} Created Successfully!";

            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["error"] = "Error creating user!";
            }

            return View();
        }
        private string GenerateEmployeeId(string fullName)
        {
            string prefix = fullName.Length >= 3 ? fullName.Substring(0, 3).ToUpper() : fullName.ToUpper();
           
            var lastEmployee = _context.Employees
                .OrderByDescending(e => e.EmployeeId)
                .FirstOrDefault();

            int nextNumber = 001; 
            string companyname = "SMT";

            if (lastEmployee != null)
            {
                string lastEmployeeId = lastEmployee.EmployeeId;
                string numericPart = new string(lastEmployeeId.Where(char.IsDigit).ToArray()); 

                if (int.TryParse(numericPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1; 
                }
            }

            return $"{prefix}{companyname}{nextNumber:D3}"; 
        }
    }
}

