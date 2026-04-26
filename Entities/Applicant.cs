namespace mmrdaconsent.API.Entities;

public class Application
{
    public int ApplicationID { get; set; }
    public int EnrollmentID { get; set; }
    public string? ReferenceNo { get; set; }
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    // Navigation
    public Enrollment Enrollment { get; set; } = null!;
    public ICollection<LandDetail> LandDetails { get; set; } = new List<LandDetail>();
    public ICollection<LocationDetail> LocationDetails { get; set; } = new List<LocationDetail>();

    public ICollection<ApplicationDocument> Documents { get; set; } = new List<ApplicationDocument>();
}