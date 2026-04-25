namespace mmrdaconsent.API.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Enrollment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EnrollmentID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public string? AadharNumber { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public int? StateID { get; set; }
    public string? State { get; set; }
    public int? CityID { get; set; }
    public string? City { get; set; }
    public string? Pincode { get; set; }
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    // Navigation
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}