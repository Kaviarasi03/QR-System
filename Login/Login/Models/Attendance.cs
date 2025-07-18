using System;
using System.ComponentModel.DataAnnotations;

namespace Login.Models
{
    public class Attendance
    {
        [Key]
        public Guid Id { get; set; }
        public int EmployeeId { get; set; }
        public DayOfWeek Day { get; set; } = DateTime.Now.DayOfWeek;
        public DateTime Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        // Computed property for total working hours
        public string TotalWorkingHours
        {
            get
            {
                if (CheckInTime.HasValue && CheckOutTime.HasValue)
                {
                    var totalHours = CheckOutTime.Value - CheckInTime.Value;
                    return string.Format("{0:D2}:{1:D2}:{2:D2}",
                        totalHours.Hours, totalHours.Minutes, totalHours.Seconds);
                }
                return "In Progress";
            }
        }

    }
}
