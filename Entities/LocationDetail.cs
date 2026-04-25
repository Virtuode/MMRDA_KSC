namespace mmrdaconsent.API.Entities;

public class LocationDetail
{
    public int LocationDetailID { get; set; }
    public int ApplicationID { get; set; }
    public int? DistrictID { get; set; }
    public string? District { get; set; }
    public int? TalukaID { get; set; }
    public string? Taluka { get; set; }
    public int? VillageID { get; set; }
    public string? Village { get; set; }
    public int? CompensationTypeID { get; set; }
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.Now;
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    // Navigation
    public Application Application { get; set; } = null!;
}