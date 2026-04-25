namespace mmrdaconsent.API.Entities;

public class LandDetail
{
    public int LandDetailID { get; set; }
    public int ApplicationID { get; set; }
    public string? GatNo { get; set; }
    public string? SurveyNo { get; set; }
    public decimal? TotalAreaHecter { get; set; }
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}