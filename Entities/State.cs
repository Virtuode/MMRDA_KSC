namespace mmrdaconsent.API.Entities;

public class State
{
    public int StateID { get; set; }
    public string StateName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public int? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    public ICollection<City> Cities { get; set; } = new List<City>();
    public ICollection<District> Districts { get; set; } = new List<District>();
}