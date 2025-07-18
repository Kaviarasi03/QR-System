using Login.Data;
using Login.Helpers;
using Login.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Login.Controllers
{
    //[Authorize(Roles ="Employee")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeeController(AppDbContext context, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }


        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            string uniqueData = $"{user.Id}-{DateTime.UtcNow.Date:yyyy-MM-dd}";
            byte[] qrCodeImage = QRCodeHelper.GenerateQRCode(uniqueData);

            ViewBag.QRCode = Convert.ToBase64String(qrCodeImage);
            return View();
        }
        
   
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult EmployeeList()
        {
            List<Employee> employees = _context.Employees.ToList();
            return View(employees);
        }
        public IActionResult EmployeeView(int id)
        {
            Employee detail = _context.Employees.FirstOrDefault(x => x.Id == id);
            return View(detail);
        }
        public IActionResult EmployeeEdit(int id)
        {
            Employee details = _context.Employees.FirstOrDefault(y => y.Id == id);
            return View(details);
        }
        [HttpPost]
        
        public IActionResult EmployeeEdit(Employee employee)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            var file = HttpContext.Request.Form.Files;
            var objFromDb = _context.Employees.AsNoTracking().FirstOrDefault(x => x.Id == employee.Id);

            if (objFromDb == null)
            {
                return NotFound();
            }

            if (file.Count > 0) 
            {
                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(webRootPath, @"Image\emppic");
                var extension = Path.GetExtension(file[0].FileName);

                
                if (!string.IsNullOrEmpty(objFromDb.Photo))
                {
                    var oldImagePath = Path.Combine(webRootPath, objFromDb.Photo.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                employee.Photo = @"\Image\emppic\" + newFileName + extension;
            }
            else
            {
                // Keep the existing image path if no new image is uploaded
                employee.Photo = objFromDb.Photo;
            }

            if (ModelState.IsValid)
            {
                _context.Employees.Update(employee);
                _context.SaveChanges();
                return RedirectToAction("EmployeeList"); 
            }

            return View(employee);
        }
        public IActionResult EmployeeDelete(int id)
        {
            Employee details = _context.Employees.FirstOrDefault(y => y.Id == id);
            return View(details);
        }
        [HttpPost]
        public IActionResult EmployeeDelete(Employee employee)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            if(!string.IsNullOrEmpty(employee.Photo))
            {
                var objFromdb = _context.Employees.AsNoTracking().FirstOrDefault(x => x.Id == employee.Id);

                if (objFromdb.Photo != null)
                {
                    var oldImagePath = Path.Combine(webRootPath, objFromdb.Photo.Trim('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }
            _context.Employees.Remove(employee);
            _context.SaveChanges();
            return RedirectToAction("EmployeeList");
        }
    }
}
