namespace Login.Models
{
    public class AttendenceEmployeeViewModel
    {
        public string EmployeeId { get; set; }      
        public string EmployeeName { get; set; } 
       
        public DayOfWeek Day { get; set; }          
        public DateTime Date { get; set; }       
        public DateTime? CheckInTime { get; set; } 
        public DateTime? CheckOutTime { get; set; } 
        public string TotalWorkingHours { get; set; }
    }
}
