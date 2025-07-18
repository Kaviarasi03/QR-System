using Login.Data;
using Login.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Login.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly object claimsTypes;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }



        [HttpPost]
        public async Task<IActionResult> MarkAttendance(string qrData)
        {
            if (string.IsNullOrEmpty(qrData))
            {
                return BadRequest("Invalid QR Code");
            }

            Console.WriteLine($"[LOG] Received QR Data: {qrData}");

            // Split QR data (assuming format: "GUID-YYYY/MM/DD")
            string[] parts = qrData.Split('-');

            if (parts.Length < 6)  // Adjust if your format is different
            {
                return BadRequest("Invalid QR Code format.");
            }

            // Extract GUID and Date
            string qrGuidString = string.Join("-", parts[0], parts[1], parts[2], parts[3], parts[4]);
            string qrDateString = string.Join("-", parts[5], parts[6], parts[7]);

            Console.WriteLine($"[LOG] Extracted GUID: {qrGuidString}");
            Console.WriteLine($"[LOG] Extracted Date: {qrDateString}");

            // Validate GUID
            if (!Guid.TryParse(qrGuidString, out Guid qrGuid))
            {
                return BadRequest($"Invalid QR Code format - GUID Parsing Failed. Extracted: {qrGuidString}");
            }

            // Validate Date
            if (!DateTime.TryParseExact(qrDateString, "yyyy/MM/dd", null, System.Globalization.DateTimeStyles.None, out DateTime qrDate))
            {
                return BadRequest($"Invalid QR Code format - Date Parsing Failed. Extracted: {qrDateString}");
            }

            // Check if QR Code is for today
            DateOnly qrDateOnly = DateOnly.FromDateTime(qrDate);
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            if (qrDateOnly != today)
            {
                return BadRequest("Invalid QR Code! This QR code is not for today.");
            }

            // Find Employee by UniqueIdentifier
            var employee = _context.Employees
                .FirstOrDefault(e => e.UniqueIdentifier.ToString().ToLower() == qrGuid.ToString().ToLower());

            if (employee == null)
            {
                Console.WriteLine($"[LOG] No Employee found for GUID: {qrGuid}");
                return BadRequest("Employee not found.");
            }

            Console.WriteLine($"[LOG] Employee Found: {employee.Name} (ID: {employee.Id})");

            // Save attendance
            return await SaveAttendance(employee.Id);
        }

        private async Task<IActionResult> SaveAttendance(int empId)
        {
            Console.WriteLine($"[LOG] Checking Attendance for Employee ID: {empId}");

            var existingAttendance = _context.AttendanceRecords
                .Where(a => a.EmployeeId == empId && a.Date == DateTime.Today)
                .OrderByDescending(a => a.CheckInTime)
                .FirstOrDefault();

            if (existingAttendance == null)
            {
                // First check-in for the day
                Console.WriteLine("[LOG] New Check-in Recorded.");
                var attendance = new Attendance
                {
                    EmployeeId = empId,
                    Date = DateTime.Today,
                    CheckInTime = DateTime.Now,
                    CheckOutTime = null
                };
                _context.AttendanceRecords.Add(attendance);
            }
            else if (existingAttendance.CheckOutTime == null)
            {
                // Check-out
                Console.WriteLine("[LOG] Check-out Recorded.");
                existingAttendance.CheckOutTime = DateTime.Now;
                _context.AttendanceRecords.Update(existingAttendance);
            }
            else
            {
                Console.WriteLine("[LOG] Attendance already marked for today.");
                return BadRequest("Attendance already marked for today.");
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("[LOG] Attendance Saved Successfully.");

            return Json(new { success = true, message = "Attendance marked successfully!" });
        

    }

        public IActionResult AttendanceHistroy()
        {
            var viewModel = (from attendance in _context.AttendanceRecords
                             join employee in _context.Employees
                             on attendance.EmployeeId equals employee.Id
                             orderby attendance.Date descending
                             select new AttendenceEmployeeViewModel
                             {
                                 EmployeeId = employee.EmployeeId,
                                 EmployeeName = employee.Name,
                                 Day = attendance.Day,
                                 Date = attendance.Date,
                                 CheckInTime = attendance.CheckInTime,
                                 CheckOutTime = attendance.CheckOutTime,
                                 TotalWorkingHours = attendance.TotalWorkingHours
                             }).ToList();

            return View(viewModel);
        }

        public IActionResult AttendanceHistoryc()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userId, out Guid employeeIdentifier))
            {
                return BadRequest("Invalid User Identifier");
            }

            var records = (from attendance in _context.AttendanceRecords
                           join employee in _context.Employees
                           on attendance.EmployeeId equals employee.Id
                           where employee.UniqueIdentifier == employeeIdentifier
                           orderby attendance.Date descending
                           select new AttendenceEmployeeViewModel
                           {
                               EmployeeId = employee.EmployeeId,
                               EmployeeName = employee.Name,
                               Date = attendance.Date,
                               Day= attendance.Day,
                               CheckInTime= attendance.CheckInTime,
                               CheckOutTime= attendance.CheckOutTime,
                               TotalWorkingHours = attendance.TotalWorkingHours

                           }).ToList();

            return View(records);
        }


        public IActionResult Scan()
        {
            return View();
        }
    }
}
