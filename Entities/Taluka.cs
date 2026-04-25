namespace mmrdaconsent.API.Entities;

public class Taluka
{
    public int TalukaID { get; set; }
    public int StateID { get; set; }
    public int DistrictID { get; set; }
    public string TalukaName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    public District District { get; set; } = null!;
    public ICollection<Village> Villages { get; set; } = new List<Village>();
}