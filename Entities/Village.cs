namespace mmrdaconsent.API.Entities;

public class Village
{
    public int VillageID { get; set; }
    public int StateID { get; set; }
    public int DistrictID { get; set; }
    public string VillageName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    public Taluka Taluka { get; set; } = null!;
}