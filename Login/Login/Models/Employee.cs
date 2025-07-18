namespace Login.Models
{
    public class Employee
    {
      
            public int Id { get; set; } // Unique Identifier (Primary Key)
            public Guid UniqueIdentifier { get; set; } // GUID for authentication
            public string EmployeeId { get; set; }
        
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Department { get; set; } 
            public string JobTitle { get; set; } 
            public string Role { get; set; } 
            public string ReportingManager { get; set; }
            public DateTime? DateOfJoining { get; set; }
            public DateTime? DateOfExit { get; set; } // Nullable for active employees
            public string EmploymentType { get; set; } 
            public string Address { get; set; }
            public bool IsActive { get; set; } 

            public string Photo {  get; set; }

           
            public bool IsDocumentSubmitted { get; set; } 
            public string SubmittedDocuments { get; set; } 
            public string PendingDocuments { get; set; } 
            public DateTime? DocumentSubmissionDate { get; set; } 
            
          
        }
    }

